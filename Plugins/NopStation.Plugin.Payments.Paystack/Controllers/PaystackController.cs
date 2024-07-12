using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.Paystack.Services;

namespace NopStation.Plugin.Payments.Paystack.Controllers
{
    public class PaystackController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly PaystackManager _paystackManager;

        public PaystackController(IOrderService orderService,
            PaystackManager paystackManager)
        {
            _orderService = orderService;
            _paystackManager = paystackManager;
        }

        public async Task<IActionResult> Callback(int orderId, string trxref)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            if (order.AuthorizationTransactionId != trxref)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            await _paystackManager.VerifyTransactionAsync(order);

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }
    }
}
