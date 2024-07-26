using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.GemVisaPayment.Models;
using Nop.Plugin.Payments.GemVisaPayment.Services;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.GemVisaPayment.Controllers
{
    public class PaymentGemVisaController : BasePaymentController
    {
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly IGemVisaPaymentService _gemVisaPaymentService;

        public PaymentGemVisaController(
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            ILogger logger,
            IWebHelper webHelper,
            IGemVisaPaymentService gemVisaPaymentService)
        {
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _gemVisaPaymentService = gemVisaPaymentService;
        }


        public IActionResult PostPaymentHandler(string gatewayReference,
            string merchantReference,
            string transactionReference)
        {
            var orderIdString = Regex.Match(merchantReference, @"\d+").Value;
            var response = _gemVisaPaymentService.VerifyPayment(new VerifyRequestBody
            {
                GatewayReference = gatewayReference,
                MerchantReference = merchantReference,
                TransactionReference = transactionReference
            });
            if (response != null)
            {
                var orderId = int.TryParse(orderIdString, out var orderIdNumber) ? orderIdNumber : 0;
                var order = _orderService.GetOrderById(orderId);
                if (order == null)
                {
                    return RedirectToRoute("Homepage");
                }
                if (response.Result.ToLowerInvariant().Equals(GemVisaPaymentDefaults.PAYMENT_STATUS_COMPLETED))
                {
                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                    }
                    //order note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order payment completed.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    //mark order as paid
                    order.CaptureTransactionId = response.TransactionReference;
                    _orderService.UpdateOrder(order);
                    _orderProcessingService.MarkOrderAsPaid(order);

                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
                else
                {
                    //order note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order payment pending.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }

            return RedirectToRoute("Homepage");
        }
        [AllowAnonymous]
        public IActionResult Webhook(string gatewayReference,
            string merchantReference,
            string transactionReference)
        {
            var orderIdString = Regex.Match(merchantReference, @"\d+").Value;
            var response = _gemVisaPaymentService.VerifyPayment(new VerifyRequestBody
            {
                GatewayReference = gatewayReference,
                MerchantReference = merchantReference,
                TransactionReference = transactionReference
            });
            if (response != null)
            {
                var orderId = int.TryParse(orderIdString, out var orderIdNumber) ? orderIdNumber : 0;
                var order = _orderService.GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound();
                }
                if (response.Result.ToLowerInvariant().Equals(GemVisaPaymentDefaults.PAYMENT_STATUS_COMPLETED))
                {
                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        return Ok();
                    }
                    //order note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order payment completed.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    //mark order as paid
                    order.CaptureTransactionId = response.TransactionReference;
                    _orderService.UpdateOrder(order);
                    _orderProcessingService.MarkOrderAsPaid(order);

                    return Ok();
                }
                else
                {
                    //order note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order payment pending.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
                return NotFound();
            }
            return NotFound();
        }

        public IActionResult RejectPayment()
        {
            return Content(string.Empty);
        }
    }
}
