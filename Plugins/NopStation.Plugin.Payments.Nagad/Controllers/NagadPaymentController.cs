using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.Nagad.Models;
using NopStation.Plugin.Payments.Nagad.Services;

namespace NopStation.Plugin.Payments.Nagad.Controllers
{
    public class NagadPaymentController : NopStationPublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly INagadPaymentService _nagadPaymentService;

        #endregion

        #region Ctor

        public NagadPaymentController(IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            INagadPaymentService nagadPaymentService)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _nagadPaymentService = nagadPaymentService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> CustomerReturn()
        {
            if (HttpContext.Request.Query.TryGetValue("payment_ref_id", out var paymentRefIdParam) &&
               HttpContext.Request.Query.TryGetValue("order_id", out var orderIdParam) &&
               HttpContext.Request.Query.TryGetValue("message", out var message))
            {
                var paymentRefId = paymentRefIdParam.FirstOrDefault();
                var orderGuid = orderIdParam.FirstOrDefault();

                var order = await _nagadPaymentService.GetOrderByAuthorizationTransactionId(paymentRefId);
                if (order != null)
                {
                    var paymentDetails = await _nagadPaymentService.VerifyPaymentAsync(paymentRefId, orderGuid);
                    if (paymentDetails != null)
                    {
                        if (paymentDetails.Status == NagadStatus.Success)
                        {
                            if (_orderProcessingService.CanMarkOrderAsPaid(order))
                            {
                                order.CaptureTransactionId = paymentDetails.PaymentRefId;
                                await _orderService.UpdateOrderAsync(order);
                                await _orderProcessingService.MarkOrderAsPaidAsync(order);
                            }
                        }

                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = order.Id,
                            Note = $"{paymentDetails.Status} Massage:{message}",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });

                        return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                    }
                }
            }

            return RedirectToRoute("Homepage");
        }

        #endregion
    }
}
