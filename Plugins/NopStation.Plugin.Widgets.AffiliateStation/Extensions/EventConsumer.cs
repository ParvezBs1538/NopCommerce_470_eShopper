using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Orders;

namespace NopStation.Plugin.Widgets.AffiliateStation.Extensions
{
    public class EventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly IWorkContext _workContext;
        private readonly IOrderCommissionService _orderCommissionService;
        private readonly ICatalogCommissionService _catalogCommissionService;
        private readonly IOrderService _orderService;
        private readonly IAffiliateCustomerService _affiliateCustomerService;
        private readonly IAffiliateService _affiliateService;
        private readonly AffiliateStationSettings _affiliateStationSettings;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;

        public EventConsumer(IWorkContext workContext,
            IOrderCommissionService orderCommissionService,
            ICatalogCommissionService catalogCommissionService,
            IOrderService orderService,
            IAffiliateCustomerService affiliateCustomerService,
            IAffiliateService affiliateService,
            AffiliateStationSettings affiliateStationSettings,
            ICategoryService categoryService,
            IManufacturerService manufacturerService)
        {
            _workContext = workContext;
            _orderCommissionService = orderCommissionService;
            _catalogCommissionService = catalogCommissionService;
            _orderService = orderService;
            _affiliateCustomerService = affiliateCustomerService;
            _affiliateService = affiliateService;
            _affiliateStationSettings = affiliateStationSettings;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
        }

        private async Task<decimal> CalculateOrderCommissionAsync(Order order, AffiliateCustomer affiliateCustomer)
        {
            var totalCommission = decimal.Zero;

            if (affiliateCustomer.OverrideCatalogCommission)
            {
                if (affiliateCustomer.UsePercentage)
                    totalCommission = order.OrderSubtotalExclTax * affiliateCustomer.CommissionPercentage / 100;
                else
                    totalCommission = (await _orderService.GetOrderItemsAsync(order.Id)).Sum(x => x.Quantity) * affiliateCustomer.CommissionAmount;
            }
            else
            {
                var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                foreach (var item in orderItems)
                {
                    var product = await _orderService.GetProductByOrderItemIdAsync(item.Id);
                    var catalogCommission = await _catalogCommissionService.GetCatalogCommissionByEntityAsync(product);
                    if (catalogCommission == null)
                    {
                        var categoryIds = (await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId)).Select(x => x.CategoryId);
                        foreach (var categoryId in categoryIds)
                        {
                            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                            if (category == null || category.Deleted || !category.Published)
                                continue;

                            catalogCommission = await _catalogCommissionService.GetCatalogCommissionByEntityAsync(category);
                            if (catalogCommission != null)
                                break;
                        }
                        if (catalogCommission == null)
                        {
                            var manufacturerIds = (await _manufacturerService.GetProductManufacturersByProductIdAsync(item.ProductId)).Select(x => x.ManufacturerId);
                            foreach (var manufacturerId in manufacturerIds)
                            {
                                var manufacturer = await _manufacturerService.GetProductManufacturerByIdAsync(manufacturerId);
                                if (manufacturer == null)
                                    continue;

                                catalogCommission = await _catalogCommissionService.GetCatalogCommissionByEntityAsync(manufacturer);
                                if (catalogCommission != null)
                                    break;
                            }
                        }
                    }

                    if (catalogCommission != null)
                    {
                        if (catalogCommission.UsePercentage)
                            totalCommission += item.PriceExclTax * catalogCommission.CommissionPercentage / 100;
                        else
                            totalCommission += item.Quantity * catalogCommission.CommissionAmount;
                    }
                    else if (_affiliateStationSettings.UseDefaultCommissionIfNotSetOnCatalog)
                    {
                        if (_affiliateStationSettings.UsePercentage)
                            totalCommission += item.PriceExclTax * _affiliateStationSettings.CommissionPercentage / 100;
                        else
                            totalCommission += item.Quantity * _affiliateStationSettings.CommissionAmount;
                    }
                }
            }
            return totalCommission;
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var affiliateCustomer = await _affiliateCustomerService.GetAffiliateCustomerByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);
            Affiliate affiliate;
            if (affiliateCustomer == null || affiliateCustomer.ApplyStatus != ApplyStatus.Approved)
            {
                affiliate = await _affiliateService.GetAffiliateByIdAsync((await _workContext.GetCurrentCustomerAsync()).AffiliateId);
                if (affiliate == null || affiliate.Deleted || !affiliate.Active)
                    return;

                affiliateCustomer = await _affiliateCustomerService.GetAffiliateCustomerByAffiliateIdAsync(affiliate.Id);
                if (affiliateCustomer == null)
                    return;
            }
            else
            {
                affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateCustomer.AffiliateId);
                if (affiliate == null || affiliate.Deleted || !affiliate.Active)
                {
                    affiliate = await _affiliateService.GetAffiliateByIdAsync((await _workContext.GetCurrentCustomerAsync()).AffiliateId);
                    if (affiliate == null || affiliate.Deleted || !affiliate.Active)
                        return;
                }

                affiliateCustomer = await _affiliateCustomerService.GetAffiliateCustomerByAffiliateIdAsync(affiliate.Id);
                if (affiliateCustomer == null || affiliateCustomer.ApplyStatus != ApplyStatus.Approved)
                    return;
            }

            if (affiliate != null && !affiliate.Deleted && affiliate.Active && affiliateCustomer.ApplyStatus == ApplyStatus.Approved)
            {
                var orderCommission = new OrderCommission()
                {
                    AffiliateId = affiliate.Id,
                    CommissionStatus = CommissionStatus.Pending,
                    OrderId = eventMessage.Order.Id,
                    TotalCommissionAmount = await CalculateOrderCommissionAsync(eventMessage.Order, affiliateCustomer),
                };
                await _orderCommissionService.InsertOrderCommissionAsync(orderCommission);

                if (eventMessage.Order.AffiliateId != affiliate.Id)
                {
                    eventMessage.Order.AffiliateId = affiliate.Id;
                    await _orderService.UpdateOrderAsync(eventMessage.Order);
                }
            }
        }

    }
}
