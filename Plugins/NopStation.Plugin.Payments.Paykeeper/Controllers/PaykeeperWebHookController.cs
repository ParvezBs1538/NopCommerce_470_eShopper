using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using NopStation.Plugin.Payments.Paykeeper.Helpers;

namespace NopStation.Plugin.Payments.Paykeeper.Controllers
{
    public class PaykeeperWebHookController : BasePublicController
    {

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly PaykeeperPaymentSettings _paykeeperPaymentSettings;
        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public PaykeeperWebHookController(IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            PaykeeperPaymentSettings paykeeperPaymentSettings,
            ILogger logger, IStoreContext storeContext, IWorkContext workContext)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _paykeeperPaymentSettings = paykeeperPaymentSettings;
            _logger = logger;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        /// <summary>
        /// Hit after successful payment 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Return()
        {
            try
            {
                var responseOrderId = Request.Form["orderid"];
                var responseClientId = Request.Form["clientid"];
                var responseOrderTotal = Request.Form["sum"];
                var responseId = Request.Form["id"];
                var responseKey = Request.Form["key"];
                await _logger.InformationAsync($"Webhook response has arrived for order id: {responseOrderId}, client id: {responseClientId}, total:{responseOrderTotal}");

                // generate key
                var secretKey = Helper.CreateMd5($"{responseId}{responseOrderTotal}{responseClientId}{responseOrderId}{_paykeeperPaymentSettings.SecretWord}").ToLower();
                var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(responseOrderId));

                if (order is { Deleted: false })
                {
                    if (order.CustomerId.ToString() == responseClientId && secretKey == responseKey)
                    {
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.CaptureTransactionId = responseId;
                            order.CaptureTransactionResult = "Payment successful";
                            await _orderService.UpdateOrderAsync(order);
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                            await _logger.InformationAsync($"Successfully payment received for order id {responseOrderId}");
                            return Ok();
                        }
                    }
                }
                return Ok();

            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return BadRequest();
            }
        }


        public async Task<IActionResult> SuccessOrFail()
        {
            var order = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                    customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
                .FirstOrDefault();

            if (order == null)
                return RedirectToAction("Index", "Home");

            await Task.Delay(10000);
            return RedirectToAction("Details", "Order", new { orderId = order.Id });
        }

    }
}
