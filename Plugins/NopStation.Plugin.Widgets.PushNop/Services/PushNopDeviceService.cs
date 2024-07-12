using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Services
{
    public class PushNopDeviceService : IPushNopDeviceService
    {
        #region Fields

        private IQueryable<WebAppDevice> _query = null;
        private readonly IWebAppDeviceService _webAppDeviceService;
        private readonly IRepository<WebAppDevice> _webAppDeviceRepository;
        private readonly ISmartGroupService _smartGroupService;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<DiscountUsageHistory> _discountUsageHistoryRepository;

        #endregion

        #region Ctor

        public PushNopDeviceService(ISmartGroupService smartGroupService,
            IRepository<WebAppDevice> webAppDeviceRepository,
            IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository,
            IWebAppDeviceService webAppDeviceService)
        {
            _webAppDeviceService = webAppDeviceService;
            _webAppDeviceRepository = webAppDeviceRepository;
            _smartGroupService = smartGroupService;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _discountUsageHistoryRepository = discountUsageHistoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
        }

        #endregion

        #region Utilities

        protected bool TryGetConditions(out IList<SmartGroupCondition> conditions, int smartGroupId)
        {
            conditions = _smartGroupService.GetSmartGroupConditionsBySmartGroupIdAsync(smartGroupId).Result;
            return conditions.Any();
        }

        #endregion

        #region Methods

        public async Task<IPagedList<WebAppDevice>> GetCampaignDevicesAsync(SmartGroupNotification smartGroupNotification, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var smartGroupId = smartGroupNotification.SmartGroupId;
            var conditions = await _smartGroupService.GetSmartGroupConditionsBySmartGroupIdAsync(smartGroupId ?? 0);

            if (smartGroupNotification.SendToAll || !smartGroupId.HasValue || !conditions.Any())
                return await _webAppDeviceService.GetWebAppDevicesAsync(storeId: smartGroupNotification.LimitedToStoreId, pageIndex: pageIndex, pageSize: pageSize);

            if (_query != null)
                return await _query.ToPagedListAsync(pageIndex, pageSize);

            var query = from wad in _webAppDeviceRepository.Table
                        join c in _customerRepository.Table on wad.CustomerId equals c.Id
                        where !c.Deleted && c.Active
                        select wad;

            foreach (var condition in conditions)
            {
                IQueryable<WebAppDevice> query1 = null;
                var skip = false;
                switch (condition.ConditionColumnType)
                {
                    case ConditionColumnType.SubscribedOnUtc:
                        if (!condition.ValueDateTime.HasValue)
                        {
                            skip = true;
                            break;
                        }

                        if (condition.ConditionType == ConditionType.GreaterThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where wad.CreatedOnUtc > condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.GreaterThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where wad.CreatedOnUtc >= condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.IsEqualTo)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where wad.CreatedOnUtc == condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where wad.CreatedOnUtc < condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where wad.CreatedOnUtc <= condition.ValueDateTime.Value
                                     select wad;
                        else
                            skip = true;
                        break;
                    case ConditionColumnType.OrderedBeforeDateUtc:
                        if (!condition.ValueDateTime.HasValue)
                        {
                            skip = true;
                            break;
                        }

                        query1 = from wad in _webAppDeviceRepository.Table
                                 where _orderRepository.Table.Count(o => !o.Deleted && o.PaymentStatusId == (int)PaymentStatus.Paid && o.CustomerId == wad.CustomerId && o.CreatedOnUtc <= condition.ValueDateTime.Value) > 0
                                 select wad;
                        break;
                    case ConditionColumnType.OrderedAfterDateUtc:
                        if (!condition.ValueDateTime.HasValue)
                        {
                            skip = true;
                            break;
                        }

                        query1 = from wad in _webAppDeviceRepository.Table
                                 where _orderRepository.Table.Count(o => !o.Deleted && o.PaymentStatusId == (int)PaymentStatus.Paid && o.CustomerId == wad.CustomerId && o.CreatedOnUtc >= condition.ValueDateTime.Value) > 0
                                 select wad;
                        break;
                    case ConditionColumnType.NeverOrdered:
                        query1 = from wad in _webAppDeviceRepository.Table
                                 where _orderRepository.Table.Count(o => o.CustomerId == wad.CustomerId) == 0
                                 select wad;
                        break;
                    case ConditionColumnType.TotalNumberOfProductsOrdered:
                        var oq = from o in _orderRepository.Table
                                 join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                                 where !o.Deleted && o.PaymentStatusId == (int)PaymentStatus.Paid
                                 select new { Order = o, OrderItem = oi };

                        if (condition.ConditionType == ConditionType.GreaterThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where oq.Where(x => x.Order.CustomerId == wad.CustomerId).Sum(x => x.OrderItem.Quantity) > condition.ValueInt
                                     select wad;
                        else if (condition.ConditionType == ConditionType.GreaterThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where oq.Where(x => x.Order.CustomerId == wad.CustomerId).Sum(x => x.OrderItem.Quantity) >= condition.ValueInt
                                     select wad;
                        else if (condition.ConditionType == ConditionType.IsEqualTo)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where oq.Where(x => x.Order.CustomerId == wad.CustomerId).Sum(x => x.OrderItem.Quantity) == condition.ValueInt
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where oq.Where(x => x.Order.CustomerId == wad.CustomerId).Sum(x => x.OrderItem.Quantity) < condition.ValueInt
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where oq.Where(x => x.Order.CustomerId == wad.CustomerId).Sum(x => x.OrderItem.Quantity) <= condition.ValueInt
                                     select wad;
                        else
                            skip = true;
                        break;
                    case ConditionColumnType.TotalSpentAmountOnOrder:
                        if (condition.ConditionType == ConditionType.GreaterThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where _orderRepository.Table.Where(x => !x.Deleted && x.CustomerId == wad.CustomerId && x.PaymentStatusId == (int)PaymentStatus.Paid).Sum(x => x.OrderTotal) > condition.ValueInt
                                     select wad;
                        else if (condition.ConditionType == ConditionType.GreaterThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where _orderRepository.Table.Where(x => !x.Deleted && x.CustomerId == wad.CustomerId && x.PaymentStatusId == (int)PaymentStatus.Paid).Sum(x => x.OrderTotal) >= condition.ValueInt
                                     select wad;
                        else if (condition.ConditionType == ConditionType.IsEqualTo)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where _orderRepository.Table.Where(x => !x.Deleted && x.CustomerId == wad.CustomerId && x.PaymentStatusId == (int)PaymentStatus.Paid).Sum(x => x.OrderTotal) == condition.ValueInt
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where _orderRepository.Table.Where(x => !x.Deleted && x.CustomerId == wad.CustomerId && x.PaymentStatusId == (int)PaymentStatus.Paid).Sum(x => x.OrderTotal) < condition.ValueInt
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     where _orderRepository.Table.Where(x => !x.Deleted && x.CustomerId == wad.CustomerId && x.PaymentStatusId == (int)PaymentStatus.Paid).Sum(x => x.OrderTotal) <= condition.ValueInt
                                     select wad;
                        else
                            skip = true;
                        break;
                    case ConditionColumnType.PurchasedFromCategoryId:
                        query1 = from wad in _webAppDeviceRepository.Table
                                 where (from o in _orderRepository.Table
                                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                                        join pcm in _productCategoryRepository.Table on oi.ProductId equals pcm.ProductId
                                        where !o.Deleted && o.PaymentStatusId == (int)PaymentStatus.Paid && o.CustomerId == wad.CustomerId && pcm.CategoryId == condition.ValueInt
                                        select o).Any()
                                 select wad;
                        break;
                    case ConditionColumnType.PurchasedFromVendorId:
                        query1 = from wad in _webAppDeviceRepository.Table
                                 where (from o in _orderRepository.Table
                                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                                        join p in _productRepository.Table on oi.ProductId equals p.Id
                                        where !o.Deleted && o.PaymentStatusId == (int)PaymentStatus.Paid && o.CustomerId == wad.CustomerId && p.VendorId == condition.ValueInt
                                        select o).Any()
                                 select wad;
                        break;
                    case ConditionColumnType.PurchasedFromManufacturerId:
                        query1 = from wad in _webAppDeviceRepository.Table
                                 where (from o in _orderRepository.Table
                                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                                        join pmm in _productManufacturerRepository.Table on oi.ProductId equals pmm.ProductId
                                        where !o.Deleted && o.PaymentStatusId == (int)PaymentStatus.Paid && o.CustomerId == wad.CustomerId && pmm.ManufacturerId == condition.ValueInt
                                        select o).Any()
                                 select wad;
                        break;
                    case ConditionColumnType.PurchasedWithDiscountId:
                        query1 = from wad in _webAppDeviceRepository.Table
                                 where (from o in _orderRepository.Table
                                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                                        join duh in _discountUsageHistoryRepository.Table on o.Id equals duh.OrderId
                                        where !o.Deleted && o.PaymentStatusId == (int)PaymentStatus.Paid && o.CustomerId == wad.CustomerId && duh.DiscountId == condition.ValueInt
                                        select o).Any()
                                 select wad;
                        break;
                    case ConditionColumnType.CustomerEmail:
                        if (condition.ConditionType == ConditionType.Contains)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.Email != null && c.Email.Contains(condition.ValueString)
                                     select wad;
                        else if (condition.ConditionType == ConditionType.DoesNotContain)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.Email != null && c.Email != condition.ValueString
                                     select wad;
                        else if (condition.ConditionType == ConditionType.IsEqualTo)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.Email != null && c.Email == condition.ValueString
                                     select wad;
                        else if (condition.ConditionType == ConditionType.StartsWith)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.Email != null && c.Email.StartsWith(condition.ValueString)
                                     select wad;
                        else if (condition.ConditionType == ConditionType.EndsWith)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.Email != null && c.Email.EndsWith(condition.ValueString)
                                     select wad;
                        else
                            skip = true;
                        break;
                    case ConditionColumnType.CustomerLastActivityDateUtc:
                        if (!condition.ValueDateTime.HasValue)
                        {
                            skip = true;
                            break;
                        }

                        if (condition.ConditionType == ConditionType.GreaterThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastActivityDateUtc > condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.GreaterThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastActivityDateUtc >= condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.IsEqualTo)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastActivityDateUtc == condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastActivityDateUtc < condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastActivityDateUtc <= condition.ValueDateTime.Value
                                     select wad;
                        else
                            skip = true;
                        break;
                    case ConditionColumnType.CustomerRegisteredOnUtc:
                        if (!condition.ValueDateTime.HasValue)
                        {
                            skip = true;
                            break;
                        }

                        if (condition.ConditionType == ConditionType.GreaterThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.CreatedOnUtc > condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.GreaterThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.CreatedOnUtc >= condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.IsEqualTo)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.CreatedOnUtc == condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.CreatedOnUtc < condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.CreatedOnUtc <= condition.ValueDateTime.Value
                                     select wad;
                        else
                            skip = true;
                        break;
                    case ConditionColumnType.CustomerLastLoginDateUtc:
                        if (!condition.ValueDateTime.HasValue)
                        {
                            skip = true;
                            break;
                        }

                        if (condition.ConditionType == ConditionType.GreaterThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastLoginDateUtc > condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.GreaterThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastLoginDateUtc >= condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.IsEqualTo)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastLoginDateUtc == condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThan)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastLoginDateUtc < condition.ValueDateTime.Value
                                     select wad;
                        else if (condition.ConditionType == ConditionType.LessThanOrEqual)
                            query1 = from wad in _webAppDeviceRepository.Table
                                     join c in _customerRepository.Table on wad.CustomerId equals c.Id
                                     where c.LastLoginDateUtc <= condition.ValueDateTime.Value
                                     select wad;
                        else
                            skip = true;
                        break;
                    default:
                        skip = true;
                        break;
                }

                if (!skip)
                {
                    switch (condition.LogicType)
                    {
                        case LogicType.And:
                            query = (from q in query join q1 in query1 on q.Id equals q1.Id select q);
                            break;
                        case LogicType.Or:
                        default:
                            query = (from q in query select q).Union(from q1 in query1 select q1);
                            break;
                    }
                }
            }

            _query = query;
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}