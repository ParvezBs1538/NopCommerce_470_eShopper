using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.bKash.Models;
using NopStation.Plugin.Payments.bKash.Services;

namespace NopStation.Plugin.Payments.bKash.Controllers
{
    public class BkashController : NopStationPublicController
    {
        private readonly IWorkContext _workContext;
        private readonly BkashPaymentSettings _bkashSettings;
        private readonly IBkashService _bkashService;
        private readonly OrderSettings _orderSettings;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;

        public BkashController(IWorkContext workContext,
            BkashPaymentSettings bkashSettings,
            IBkashService bkashService,
            OrderSettings orderSettings,
            IOrderService orderService,
            ILogger logger,
            ICustomerService customerService)
        {
            _workContext = workContext;
            _bkashSettings = bkashSettings;
            _bkashService = bkashService;
            _orderSettings = orderSettings;
            _orderService = orderService;
            _logger = logger;
            _customerService = customerService;
        }

        public async Task<IActionResult> Pay(int orderId)
        {
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId
                || order.PaymentMethodSystemName != "NopStation.Plugin.Payments.bKash")
            {
                return RedirectToRoute("Homepage");
            }

            if (order.PaymentStatus != PaymentStatus.Pending || order.OrderStatus == OrderStatus.Cancelled)
            {
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }

            var model = new PaymentViewModel()
            {
                Intent = "sale",
                OrderId = order.Id,
                OrderTotal = order.OrderTotal,
                IsSandBox = _bkashSettings.UseSandbox,
                CustomOrderNumber = order.CustomOrderNumber
            };

            return View("~/Plugins/NopStation.Plugin.Payments.bKash/Views/BkashPay.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(int orderId)
        {
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId
                || order.PaymentMethodSystemName != "NopStation.Plugin.Payments.bKash")
            {
                return Json(new
                {
                    status = false,
                    redirect = Url.RouteUrl("Homepage")
                });
            }

            if (order.PaymentStatus != PaymentStatus.Pending || order.OrderStatus == OrderStatus.Cancelled)
            {
                return Json(new
                {
                    status = false,
                    redirect = Url.RouteUrl("OrderDetails", new { orderId = order.Id })
                });
            }

            var response = await _bkashService.CreatePaymentAsync(order);

            if (!string.IsNullOrWhiteSpace(response.ErrorCode))
            {
                await _logger.ErrorAsync($"Error code: {response.ErrorCode}, Message: {response.ErrorMessage}");
                return Json(new
                {
                    status = false,
                    message = response.ErrorMessage
                });
            }

            return Json(new
            {
                status = true,
                data = JsonConvert.SerializeObject(response)
            });
        }

        [HttpPost]
        public async Task<IActionResult> ExecutePayment(int orderId)
        {
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId || order.PaymentMethodSystemName != "NopStation.bKashPayment")
            {
                return Json(new
                {
                    status = false,
                    redirect = Url.RouteUrl("Homepage")
                });
            }

            if (order.PaymentStatus != PaymentStatus.Pending || order.OrderStatus == OrderStatus.Cancelled)
            {
                return Json(new
                {
                    status = false,
                    redirect = Url.RouteUrl("OrderDetails", new { orderId = order.Id })
                });
            }

            var response = await _bkashService.ExecutePaymentAsync(order);

            if (!string.IsNullOrWhiteSpace(response.ErrorCode))
            {
                await _logger.ErrorAsync($"Error code: {response.ErrorCode}, Message: {response.ErrorMessage}");
                return Json(new
                {
                    status = false,
                    message = response.ErrorMessage
                });
            }

            return Json(new
            {
                status = true,
                data = response
            });
        }
    }
}
