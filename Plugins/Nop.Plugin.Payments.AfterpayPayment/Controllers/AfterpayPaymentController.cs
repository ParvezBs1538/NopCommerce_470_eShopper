using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.AfterpayPayment.Services;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.AfterpayPayment.Controllers
{
    public class AfterpayPaymentController : BasePaymentController
    {
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IAfterpayPaymentService _afterpayRequestService;

        public AfterpayPaymentController(
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IAfterpayPaymentService afterpayRequestService)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _afterpayRequestService = afterpayRequestService;
        }

        [HttpGet]
        public IActionResult PostPaymentHandler(string status, string orderToken)
        {
            if (!Request.QueryString.HasValue)
            {
                return RedirectToRoute("Homepage");
            }
            var response = _afterpayRequestService.GetPaymentStatus(orderToken);
            if (response != null)
            {
                var orderId = Convert.ToInt32(response.MerchantReference);
                var order = _orderService.GetOrderById(orderId);
                if (order == null)
                {
                    return RedirectToRoute("Homepage");
                }
                if (response.Status == AfterpayPaymentDefaults.APPROVED
                 && response.PaymentState == AfterpayPaymentDefaults.CAPTURED)
                {
                    //order note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order payment completed.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    //mark order as paid
                    order.CaptureTransactionId = response.Id;
                    _orderService.UpdateOrder(order);
                    _orderProcessingService.MarkOrderAsPaid(order);

                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
                else if (response.Status == AfterpayPaymentDefaults.DECLINED
                 && response.PaymentState == AfterpayPaymentDefaults.CAPTURE_DECLINED)
                {
                    _orderService.InsertOrderNote(new OrderNote
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
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = "Order payment pending.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
                //check order status
                order.AuthorizationTransactionId = response.Token;
                order.OrderStatus = OrderStatus.Pending;
                _orderService.UpdateOrder(order);
                _orderProcessingService.CheckOrderStatus(order);
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }

            return RedirectToRoute("Homepage");
        }

        [HttpGet]
        public IActionResult CancelPayment(string status, string orderToken)
        {
            if (!Request.QueryString.HasValue)
            {
                return RedirectToRoute("Homepage");
            }
            var orderDetailsUrl = _afterpayRequestService.CancelPayment(orderToken);

            if (!string.IsNullOrEmpty(orderDetailsUrl))
            {
                return Redirect(orderDetailsUrl);
            }
            return RedirectToRoute("Homepage");
        }
    }
}
