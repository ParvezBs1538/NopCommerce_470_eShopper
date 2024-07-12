using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.OrderRatings.Models;
using NopStation.Plugin.Widgets.OrderRatings.Services;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Web.Models.Order;

namespace NopStation.Plugin.Widgets.OrderRatings.Components
{
    public class OrderRatingDetailsViewComponent : NopStationViewComponent
    {
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IOrderRatingService _orderRatingService;
        private readonly IOrderService _orderService;
        private readonly OrderRatingSettings _orderRatingSettings;

        public OrderRatingDetailsViewComponent(IDateTimeHelper dateTimeHelper,
            OrderRatingSettings orderRatingSettings,
            IOrderRatingService orderRatingService,
            IOrderService orderService)
        {
            _orderRatingService = orderRatingService;
            _orderService = orderService;
            _orderRatingSettings = orderRatingSettings;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (additionalData.GetType() != typeof(OrderDetailsModel))
                return Content("");

            var om = additionalData as OrderDetailsModel;
            var order = await _orderService.GetOrderByIdAsync(om.Id);
            if(order == null)
                return Content("");

            var model = new OrderRatingModel
            {
                Id = om.Id,
                CreatedOn = om.CreatedOn,
                CustomOrderNumber = om.CustomOrderNumber,
                OrderStatus = om.OrderStatus,
                ShippingStatus = om.ShippingStatus,
                OrderTotal = om.OrderTotal,
                PaymentStatus = om.PaymentMethodStatus,
                OrderStatusEnum  = order.OrderStatus
            };

            var orderRating = await _orderRatingService.GetOrderRatingByOrderIdAsync(om.Id);
            if (orderRating != null)
            {
                model.Rating = orderRating.Rating;
                model.ShowOrderRatedDateInDetailsPage = _orderRatingSettings.ShowOrderRatedDateInDetailsPage;
                if (orderRating.RatedOnUtc.HasValue)
                    model.RatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(orderRating.RatedOnUtc.Value, DateTimeKind.Utc);
            }

            return View(model);
        }
    }
}
