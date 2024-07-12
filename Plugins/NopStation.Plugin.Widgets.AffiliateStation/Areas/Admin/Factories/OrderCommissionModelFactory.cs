using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Directory;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using Nop.Services;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories
{
    public class OrderCommissionModelFactory : IOrderCommissionModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly IOrderCommissionService _orderCommissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public OrderCommissionModelFactory(IDateTimeHelper dateTimeHelper,
            IAffiliateService affiliateService,
            ICustomerService customerService,
            IOrderCommissionService orderCommissionService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IPriceFormatter priceFormatter,
            IOrderService orderService)
        {
            _dateTimeHelper = dateTimeHelper;
            _affiliateService = affiliateService;
            _customerService = customerService;
            _orderCommissionService = orderCommissionService;
            _localizationService = localizationService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _priceFormatter = priceFormatter;
            _orderService = orderService;
        }

        #endregion

        #region Utilities

        protected async Task PrepareCommissionStatusesAsync(IList<SelectListItem> items, bool excludeDefaultItem = false)
        {
            var availablePositionItems = await CommissionStatus.Pending.ToSelectListAsync(false);
            foreach (var positionItem in availablePositionItems)
            {
                items.Add(positionItem);
            }

            if (!excludeDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }

        #endregion

        #region Methods

        public async Task<OrderCommissionSearchModel> PrepareOrderCommissionSearchModelAsync(OrderCommissionSearchModel searchModel)
        {
            await PrepareCommissionStatusesAsync(searchModel.AvailableCommissionStatuses);
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);

            searchModel.CommissionStatusIds.Add(0);
            searchModel.OrderStatusIds.Add(0);
            searchModel.PaymentStatusIds.Add(0);

            return searchModel;
        }

        public async Task<OrderCommissionListModel> PrepareOrderCommissionListModelAsync(OrderCommissionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var commissionStatusIds = (searchModel.CommissionStatusIds?.Contains(0) ?? true) ? null : searchModel.CommissionStatusIds.ToList();
            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
            var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
            var createdFromUtc = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var createdToUtc = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            var orderCommissions = await _orderCommissionService.SearchOrderCommissionsAsync(
                firstName: searchModel.AffiliateFirstName,
                lastName: searchModel.AffiliateLastName,
                csIds: commissionStatusIds,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                startDateUtc: createdFromUtc,
                endDateUtc: createdToUtc,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = await new OrderCommissionListModel().PrepareToGridAsync(searchModel, orderCommissions.Item1, () =>
            {
                //fill in model values from the entity
                return orderCommissions.Item1.SelectAwait(async orderCommission =>
                {
                    var orderCommissionModel = await PrepareOrderCommissionModelAsync(null, orderCommission, true);

                    return orderCommissionModel;
                });
            });

            return model;
        }

        public async Task<OrderCommissionModel> PrepareOrderCommissionModelAsync(OrderCommissionModel model,
            OrderCommission orderCommission, bool excludeProperties = false)
        {
            if (orderCommission != null)
            {
                var order = await _orderService.GetOrderByIdAsync(orderCommission.OrderId);
                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

                //fill in model values from the entity
                model ??= orderCommission.ToModel<OrderCommissionModel>();
                model.TotalCommissionAmountStr = await _priceFormatter.FormatPriceAsync(orderCommission.TotalCommissionAmount, true, false);
                model.CommissionStatus = await _localizationService.GetLocalizedEnumAsync(orderCommission.CommissionStatus);
                model.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);
                model.PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
                model.OrderStatusId = order.OrderStatusId;
                model.PaymentStatusId = order.PaymentStatusId;

                model.CustomerId = order.CustomerId;
                model.CustomerName = await _customerService.GetCustomerFullNameAsync(customer);
                model.CustomerEmail = customer.Email;

                model.CustomerInfo = await _customerService.IsRegisteredAsync(customer) ? customer.Email : await _localizationService.GetResourceAsync("Admin.Customers.Guest");

                model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);

                if (orderCommission.CommissionPaidOn.HasValue)
                    model.CommissionPaidOn = await _dateTimeHelper.ConvertToUserTimeAsync(orderCommission.CommissionPaidOn.Value, DateTimeKind.Utc);

                var affiliate = await _affiliateService.GetAffiliateByIdAsync(orderCommission.AffiliateId);
                if (affiliate != null)
                {
                    model.AffiliateId = affiliate.Id;
                    model.AffiliateName = await _affiliateService.GetAffiliateFullNameAsync(affiliate);
                }
            }

            //prepare localized models
            if (!excludeProperties)
                await PrepareCommissionStatusesAsync(model.AvailableCommissionStatuses, true);

            return model;
        }

        public async Task<OrderCommissionAggreratorModel> PrepareCommissionAggregatorModelAsync(OrderCommissionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var commissionStatusIds = (searchModel.CommissionStatusIds?.Contains(0) ?? true) ? null : searchModel.CommissionStatusIds.ToList();
            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
            var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
            var createdFromUtc = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var createdToUtc = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            decimal totalCommission, payableCommission, paidCommission;

            (_, totalCommission, payableCommission, paidCommission) = await _orderCommissionService.SearchOrderCommissionsAsync(
                loadCommission: true,
                firstName: searchModel.AffiliateFirstName,
                lastName: searchModel.AffiliateLastName,
                csIds: commissionStatusIds,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                startDateUtc: createdFromUtc,
                endDateUtc: createdToUtc,
                pageSize: 1,
                pageIndex: 0);

            var model = new OrderCommissionAggreratorModel()
            {
                aggregatortotal = await _priceFormatter.FormatPriceAsync(totalCommission, true, false),
                aggregatorpaid = await _priceFormatter.FormatPriceAsync(paidCommission, true, false),
                aggregatorpayable = await _priceFormatter.FormatPriceAsync(payableCommission, true, false)
            };

            return model;
        }

        #endregion
    }
}
