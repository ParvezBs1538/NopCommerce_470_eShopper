using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Widgets.OrderRatings.Domain;
using NopStation.Plugin.Widgets.OrderRatings.Factories;
using NopStation.Plugin.Widgets.OrderRatings.Models;
using NopStation.Plugin.Widgets.OrderRatings.Services;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace NopStation.Plugin.Widgets.OrderRatings.Controllers
{
    public class OrderRatingController : BasePluginController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IOrderRatingService _orderRatingService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IOrderRatingModelFactory _orderRatingModelFactory;
        private readonly OrderRatingSettings _orderRatingSettings;

        #endregion

        #region Ctor

        public OrderRatingController(IWorkContext workContext,
            IOrderRatingService orderRatingService,
            ICustomerService customerService,
            IOrderService orderService,
            IOrderRatingModelFactory orderRatingModelFactory,
            OrderRatingSettings orderRatingSettings)
        {
            _workContext = workContext;
            _orderRatingService = orderRatingService;
            _customerService = customerService;
            _orderService = orderService;
            _orderRatingModelFactory = orderRatingModelFactory;
            _orderRatingSettings = orderRatingSettings;
        }

        #endregion

        #region Utilities

        protected async Task UpdateRating(Order order, int rating, string note)
        {
            var orderRating = await _orderRatingService.GetOrderRatingByOrderIdAsync(order.Id);
            if (orderRating != null && orderRating.RatedOnUtc.HasValue)
                return;

            if (orderRating == null)
            {
                orderRating = new OrderRating()
                {
                    Note = note,
                    OrderId = order.Id,
                    RatedOnUtc = DateTime.UtcNow,
                    Rating = rating
                };
                await _orderRatingService.InsertOrderRatingAsync(orderRating);
            }
            else
            {
                orderRating.Note = note;
                orderRating.RatedOnUtc = DateTime.UtcNow;
                orderRating.Rating = rating;
                await _orderRatingService.UpdateOrderRatingAsync(orderRating);
            }
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> LoadRateableOrders()
        {
            if (!_orderRatingSettings.ShowOrderRatedDateInDetailsPage)
                return Json(new { Result = false });

            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Json(new { Result = false });

            var orders = await _orderRatingService.GetAllOrdersAsync(
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id,
                ignoreRated: true,
                ignoreSkipped: true,
                rateable: true);

            if (!orders.Any())
                return Json(new { Result = false });

            var model = await _orderRatingModelFactory.PrepareRateableOrderListModelAsync(orders);

            var html = await RenderPartialViewToStringAsync("LoadRateableOrders", model);
            return Json(new { Result = true, html = html });
        }

        [HttpPost]
        public async Task<IActionResult> SaveRating(OrderRatingModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return RedirectToRoute("Homepage");

            var order = await _orderService.GetOrderByIdAsync(model.Id);
            if (order == null || order.CustomerId != customer.Id)
                return RedirectToRoute("Homepage");

            await UpdateRating(order, model.Rating, model.Note);

            return RedirectToRoute("OrderDetails", new { orderId = order.Id });
        }

        [HttpPost]
        public async Task<IActionResult> SaveRatings(IFormCollection form)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return RedirectToRoute("Homepage");

            foreach (var key in form.Keys)
            {
                if (key.StartsWith("OrderRating_Id_"))
                {
                    if (!int.TryParse(form[key], out var orderId))
                        continue;

                    var order = await _orderService.GetOrderByIdAsync(orderId);
                    if (order == null || order.CustomerId != customer.Id)
                        continue;

                    if (!int.TryParse(form[$"OrderRating_Rate_{orderId}"], out var rating))
                        continue;

                    var note = form.ContainsKey($"OrderRating_Note_{orderId}") ? form[$"OrderRating_Note_{orderId}"].ToString() : "";
                    await UpdateRating(order, rating, note);
                }
            }

            return RedirectToRoute("Homepage");
        }

        [HttpPost]
        public async Task<IActionResult> SkipRatings(IFormCollection form)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Json(new { Result = false });

            foreach (var key in form.Keys)
            {
                if (key.StartsWith("OrderRating_Id_"))
                {
                    if (!int.TryParse(form[key], out var orderId))
                        continue;

                    var orderRating = await _orderRatingService.GetOrderRatingByOrderIdAsync(orderId);
                    if (orderRating != null)
                        continue;

                    orderRating = new OrderRating()
                    {
                        OrderId = orderId,
                    };
                    await _orderRatingService.InsertOrderRatingAsync(orderRating);
                }
            }

            return Json(new { Result = true });
        }

        #endregion
    }
}
