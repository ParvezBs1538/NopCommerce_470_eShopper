using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.Instamojo.Services;

namespace NopStation.Plugin.Payments.Instamojo.Controllers
{
    public class InstamojoController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly InstamojoManager _instamojoManager;

        public InstamojoController(IOrderService orderService,
            InstamojoManager instamojoManager)
        {
            _orderService = orderService;
            _instamojoManager = instamojoManager;
        }

        public async Task<IActionResult> Callback(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Pending)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            await _instamojoManager.VerifyPaymentAsync(order);

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }
    }
}
