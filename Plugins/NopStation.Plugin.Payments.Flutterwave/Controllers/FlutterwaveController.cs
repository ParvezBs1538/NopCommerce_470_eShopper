using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.Flutterwave.Services;

namespace NopStation.Plugin.Payments.Flutterwave.Controllers
{
    public class FlutterwaveController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly FlutterwaveManager _flutterwaveManager;
        private readonly IOrderProcessingService _orderProcessingService;

        public FlutterwaveController(IOrderService orderService,
            FlutterwaveManager flutterwaveManager,
            IOrderProcessingService orderProcessingService)
        {
            _orderService = orderService;
            _flutterwaveManager = flutterwaveManager;
            _orderProcessingService = orderProcessingService;
        }

        public async Task<IActionResult> Return(int orderId, string status, string tx_ref, string transaction_id)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            if (order.OrderGuid != System.Guid.Parse(tx_ref))
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Pending)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            if (status?.ToLower() != "successful")
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            await _orderProcessingService.MarkAsAuthorizedAsync(order);

            await _flutterwaveManager.VerifyPaymentAsync(order, transaction_id);

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }
    }
}
