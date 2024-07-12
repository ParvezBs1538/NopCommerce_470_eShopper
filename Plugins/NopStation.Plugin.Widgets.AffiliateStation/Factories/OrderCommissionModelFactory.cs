using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Payments;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;

namespace NopStation.Plugin.Widgets.AffiliateStation.Factories
{
    public class OrderCommissionModelFactory : IOrderCommissionModelFactory
    {
        #region Fields

        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderCommissionService _orderCommissionService;
        private readonly AffiliateStationSettings _affiliateStationSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public OrderCommissionModelFactory(IPriceFormatter priceFormatter,
            IOrderCommissionService orderCommissionService,
            AffiliateStationSettings affiliateStationSettings,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IOrderService orderService)
        {
            _priceFormatter = priceFormatter;
            _orderCommissionService = orderCommissionService;
            _affiliateStationSettings = affiliateStationSettings;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _orderService = orderService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public async Task<AffiliatedOrderModel> PrepareAffiliatedOrderSummaryModelAsync(AffiliateCustomer affiliateCustomer, AffiliatedOrderPagingFilteringModel command)
        {
            if (affiliateCustomer == null)
                throw new ArgumentNullException(nameof(affiliateCustomer));

            var model = new AffiliatedOrderModel();

            var orderCommissions = await _orderCommissionService.SearchOrderCommissionsAsync(
                loadCommission: true,
                affiliateId: affiliateCustomer.AffiliateId,
                pageIndex: command.PageIndex,
                pageSize: _affiliateStationSettings.AffiliatePageOrderPageSize);

            model.TotalCommission = await _priceFormatter.FormatPriceAsync(orderCommissions.Item2, true, false);
            model.PayableCommission = await _priceFormatter.FormatPriceAsync(orderCommissions.Item3, true, false);
            model.PaidCommission = await _priceFormatter.FormatPriceAsync(orderCommissions.Item4, true, false);
            
            foreach (var orderCommission in orderCommissions.Item1)
            {
                var order = await _orderService.GetOrderByIdAsync(orderCommission.OrderId);
                var commissionPaidOn = orderCommission.CommissionStatus != CommissionStatus.Pending && orderCommission.CommissionPaidOn.HasValue ?
                    await _dateTimeHelper.ConvertToUserTimeAsync(orderCommission.CommissionPaidOn.Value, DateTimeKind.Utc) : (DateTime?)null;

                var paidCommissionAmount = "";
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    if (orderCommission.CommissionStatus == CommissionStatus.Paid)
                        paidCommissionAmount = await _priceFormatter.FormatPriceAsync(orderCommission.TotalCommissionAmount, true, false);
                    else if (orderCommission.CommissionStatus == CommissionStatus.PartiallyPaid)
                        paidCommissionAmount = await _priceFormatter.FormatPriceAsync(orderCommission.PartialPaidAmount, true, false);
                }

                model.Orders.Add(new AffiliatedOrderModel.OrderModel()
                {
                    CommissionStatus = await _localizationService.GetLocalizedEnumAsync(orderCommission.CommissionStatus),
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                    CommissionPaidOn = commissionPaidOn,
                    OrderId = order.Id,
                    OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                    PaidCommissionAmount = paidCommissionAmount,
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    TotalCommissionAmount = await _priceFormatter.FormatPriceAsync(orderCommission.TotalCommissionAmount, true, false)
                });
            }

            model.PagingFilteringContext.LoadPagedList(orderCommissions.Item1);

            return model;
        }

        #endregion
    }
}
