using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace NopStation.Plugin.Payments.MPay24.Controllers
{
    public class PaymentMPay24Controller : NopStationPublicController
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor
        public PaymentMPay24Controller(ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task<ActionResult> PDTHandler()
        {
            string orderGuid = Request.Query["orderId"];
            string transactionId = Request.Query["MPAYTID"];
            var order = await _orderService.GetOrderByGuidAsync(Guid.Parse(orderGuid));
            var data = "";

            foreach (var queryItem in Request.Query)
            {
                data += queryItem.Key + " = " + queryItem.Value + Environment.NewLine;
            }

            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Information, "QueryString = " + data);

            if (order == null || order.Id == 0)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            else
            {
                //order note
                if (!string.IsNullOrEmpty(transactionId))
                    order.AuthorizationTransactionId = transactionId;

                var orderNote = new OrderNote()
                {
                    OrderId = order.Id,
                    Note = "Order Placed",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _orderService.InsertOrderNoteAsync(orderNote);
                await _orderService.UpdateOrderAsync(order);

                //mark order as paid
                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    order.AuthorizationTransactionId = transactionId;
                    await _orderService.UpdateOrderAsync(order);

                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
                }
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
        }

        public async Task<ActionResult> IPNHandler()
        {
            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Information, "From IPN");
            try
            {
                byte[] parameters;
                using (var stream = new MemoryStream())
                {
                    Request.Body.CopyTo(stream);
                    parameters = stream.ToArray();
                }
                var strRequest = Encoding.ASCII.GetString(parameters);

                await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Information, "IPN Data= " + strRequest);
            }
            catch (Exception) { }

            if (true)
            {
                string orderGuid = Request.Query["orderId"];
                string transactionId = Request.Query["MPAYTID"];
                string status = Request.Query["STATUS"];
                var order = await _orderService.GetOrderByGuidAsync(Guid.Parse(orderGuid));
                if (order != null && order.Id > 0 && !string.IsNullOrEmpty(status) && (status.ToUpperInvariant() == "BILLED" || status.ToUpperInvariant() == "RESERVED"))
                {
                    if (!string.IsNullOrEmpty(transactionId))
                        order.AuthorizationTransactionId = transactionId;
                    var orderNote = new OrderNote()
                    {
                        OrderId = order.Id,
                        Note = "Order Placed",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    await _orderService.InsertOrderNoteAsync(orderNote);
                    await _orderService.UpdateOrderAsync(order);

                    //mark order as paid
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = transactionId;
                        await _orderService.UpdateOrderAsync(order);
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);
                    }
                }
            }

            return Content("");
        }

        public async Task<IActionResult> CancelOrder()
        {
            var orders = await _orderService.SearchOrdersAsync(storeId: _storeContext.GetCurrentStoreAsync().Id,
                customerId: _workContext.GetCurrentCustomerAsync().Id, pageSize: 1);
            var order = orders.FirstOrDefault();
            if (order != null)
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });

            return RedirectToRoute("HomePage");
        }

        #endregion
    }
}
