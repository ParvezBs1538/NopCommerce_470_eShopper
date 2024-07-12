using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Widgets.OrderRatings.Models;
using NopStation.Plugin.Widgets.OrderRatings.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;

namespace NopStation.Plugin.Widgets.OrderRatings.Factories
{
    public class OrderRatingModelFactory : IOrderRatingModelFactory
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderRatingService _orderRatingService;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Ctor

        public OrderRatingModelFactory(IWorkContext workContext,
            ILocalizationService localizationService,
            IOrderRatingService orderRatingService,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter) 
        {
            _workContext = workContext;
            _localizationService = localizationService;
            _orderRatingService = orderRatingService;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
        }

        #endregion

        #region Methods

        public async Task<OrderRatingModel> PrepareOrderRatingModelAsync(Order order)
        {
            var model = new OrderRatingModel()
            {
                OrderId = order.Id
            };

            var orderRating = await _orderRatingService.GetOrderRatingByOrderIdAsync(order.Id);
            if (orderRating == null)
                return model;

            if (orderRating.RatedOnUtc.HasValue)
            {
                model.RatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(orderRating.RatedOnUtc.Value, DateTimeKind.Utc);
                model.Rating = orderRating.Rating;
            }

            return model;
        }

        public async Task<RateableOrderListModel> PrepareRateableOrderListModelAsync(IEnumerable<Order> orders)
        {
            var model = new RateableOrderListModel();
            foreach (var order in orders)
            {
                var orderModel = new RateableOrderListModel.OrderDetailsModel
                {
                    Id = order.Id,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusEnum = order.OrderStatus,
                    OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                    CustomOrderNumber = order.CustomOrderNumber
                };
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, (await _workContext.GetWorkingLanguageAsync()).Id);

                model.Orders.Add(orderModel);
            }
            return model;
        }

        #endregion
    }
}
