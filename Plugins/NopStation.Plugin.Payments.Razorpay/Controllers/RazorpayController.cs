using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.Razorpay.Services;

namespace NopStation.Plugin.Payments.Razorpay.Controllers
{
    public class RazorpayController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly RazorpayManager _razorpayManager;

        public RazorpayController(IOrderService orderService,
            RazorpayManager razorpayManager)
        {
            _orderService = orderService;
            _razorpayManager = razorpayManager;
        }

        public async Task<IActionResult> Callback(int orderId, string razorpay_payment_link_id)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            if (order.AuthorizationTransactionId != razorpay_payment_link_id)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Pending)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            await _razorpayManager.VerifyPaymentAsync(order, Request.Query);

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }
    }
}
