using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.CBL.Services;

namespace NopStation.Plugin.Payments.CBL.Controllers
{
    public class CBLPaymentController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICBLPaymentService _cBLPaymentService;

        public CBLPaymentController(IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IHttpContextAccessor httpContextAccessor,
            ICBLPaymentService cBLPaymentService)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _httpContextAccessor = httpContextAccessor;
            _cBLPaymentService = cBLPaymentService;
        }

        [HttpGet]
        public async Task<IActionResult> PostPaymentHandler()
        {
            if (!Request.QueryString.HasValue)
            {
                return RedirectToRoute("Homepage");
            }
            var searchParams = _httpContextAccessor.HttpContext.Request.Query;
            if (searchParams.TryGetValue("STATUS", out var status))
            {
                if (searchParams.TryGetValue("token", out var transactionId)
                    && searchParams.TryGetValue("sessionID", out var sessionID))
                {
                    var order = await _cBLPaymentService.GetTransactionByTransactionCodeAsync(transactionId);
                    if (order == null || order.Deleted)
                    {
                        return RedirectToRoute("Homepage");
                    }
                    if (status != "APPROVED")
                    {
                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = order.Id,
                            Note = "Order payment not completed.",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        return RedirectToRoute("OrderDetails", new { orderId = order.Id });
                    }

                    var orderResponse = await _cBLPaymentService.GetOrderDetailsAsync(order, transactionId, sessionID);
                    if (orderResponse.OrderStatus == "APPROVED")
                    {
                        var paidAmount = Convert.ToDecimal(orderResponse.Amount);

                        await _orderService.UpdateOrderAsync(order);

                        order.CaptureTransactionResult = orderResponse.OrderStatus;
                        order.CaptureTransactionId = sessionID;
                        order.AuthorizationTransactionId = orderResponse.OrderId;
                        order.PaidDateUtc = DateTime.UtcNow;
                        await _orderService.UpdateOrderAsync(order);
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);

                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = order.Id,
                            Note = "Order payment completed.",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                    }
                }
            }
            return RedirectToRoute("Homepage");
        }
    }
}
