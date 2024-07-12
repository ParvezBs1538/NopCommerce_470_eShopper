using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.Afterpay.Services;

namespace NopStation.Plugin.Payments.Afterpay.Controllers
{
    public class AfterpayPaymentController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IAfterpayPaymentService _afterpayRequestService;
        private readonly ILogger _logger;

        public AfterpayPaymentController(
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IAfterpayPaymentService afterpayRequestService,
            ILogger logger)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _afterpayRequestService = afterpayRequestService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> PostPaymentHandler(string status, string orderToken)
        {
            if (!Request.QueryString.HasValue)
            {
                return RedirectToRoute("Homepage");
            }
            var response = await _afterpayRequestService.GetPaymentStatusAsync(orderToken);
            if (response != null)
            {
                var orderId = Convert.ToInt32(response.MerchantReference);
                var order = await _orderService.GetOrderByIdAsync(orderId);

                if (response.Status == AfterpayPaymentDefaults.APPROVED
                 && response.PaymentState == AfterpayPaymentDefaults.CAPTURED)
                {
                    //order note
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order payment completed.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    //mark order as paid
                    order.CaptureTransactionId = response.Id;
                    await _orderService.UpdateOrderAsync(order);
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);

                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
                else if (response.Status == AfterpayPaymentDefaults.DECLINED
                 && response.PaymentState == AfterpayPaymentDefaults.CAPTURE_DECLINED)
                {
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order payment DECLINED.",
                        DisplayToCustomer = true,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
                else
                {
                    //order note
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order payment not completed.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }

                order.AuthorizationTransactionId = response.Token;
                await _orderService.UpdateOrderAsync(order);
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }

            return RedirectToRoute("Homepage");
        }

        [HttpGet]
        public async Task<IActionResult> CancelPayment(string status, string orderToken)
        {
            try
            {
                if (!Request.QueryString.HasValue)
                    return RedirectToRoute("Homepage");

                var response = await _afterpayRequestService.GetCancelPaymentResponseAsync(orderToken);

                if (string.IsNullOrWhiteSpace(response.MerchantReference))
                    return RedirectToRoute("Homepage");

                return RedirectToRoute("OrderDetails", new { orderId = Convert.ToInt32(response.MerchantReference) });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return RedirectToRoute("Homepage");
            }
        }
    }
}
