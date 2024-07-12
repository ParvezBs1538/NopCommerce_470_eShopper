using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.OrderRatings.Areas.Admin.Models;
using NopStation.Plugin.Widgets.OrderRatings.Services;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Models.Orders;

namespace NopStation.Plugin.Widgets.OrderRatings.Areas.Admin.Components
{
    public class OrderRatingViewComponent : NopStationViewComponent
    {
        private readonly IOrderRatingService _orderRatingService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public OrderRatingViewComponent(IOrderRatingService orderRatingService,
            IDateTimeHelper dateTimeHelper)
        {
            _orderRatingService = orderRatingService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (additionalData.GetType() != typeof(OrderModel))
                return Content("");

            var orderModel = additionalData as OrderModel;
            var orderRating = await _orderRatingService.GetOrderRatingByOrderIdAsync(orderModel.Id);
            if (orderRating == null)
                return Content("");

            var model = new OrderRatingModel()
            {
                Id = orderRating.Id,
                Note = orderRating.Note,
                Rating = orderRating.Rating,
                RatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(orderRating.RatedOnUtc.Value, DateTimeKind.Utc)
            };

            return View(model);
        }
    }
}
