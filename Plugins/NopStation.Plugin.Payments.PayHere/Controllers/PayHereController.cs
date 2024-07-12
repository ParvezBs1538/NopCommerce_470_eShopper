using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.PayHere.Services;

namespace NopStation.Plugin.Payments.PayHere.Controllers
{
    public class PayHereController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly PayHereManager _payHereManager;

        public PayHereController(IOrderService orderService,
            PayHereManager payHereManager)
        {
            _orderService = orderService;
            _payHereManager = payHereManager;
        }

        [HttpPost]
        public async Task<IActionResult> Notify(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Pending)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            var status_code = Request.Query["status_code"].ToString();
            if (status_code == "2")
                await _payHereManager.VerifyPaymentAsync(order, Request.Query);

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        public async Task<IActionResult> Return(int orderId, string status)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Pending)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            if (status?.ToLower() == "return")
            {
                order.AuthorizationTransactionResult = "authorized";
                await _orderService.UpdateOrderAsync(order);
            }

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }
    }
}
