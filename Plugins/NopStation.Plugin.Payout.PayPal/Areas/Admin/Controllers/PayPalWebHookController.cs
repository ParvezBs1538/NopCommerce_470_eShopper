using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Web.Controllers;
using NopStation.Plugin.Widgets.VendorCommission.Domain;
using NopStation.Plugin.Widgets.VendorCommission.Services;

namespace NopStation.Plugin.Payout.PayPal.Areas.Admin.Controllers
{
    public class PayPalWebHookController : BasePublicController
    {
        private readonly IVendorOrderInfoService _vendorOrderInfoService;
        private readonly IVendorPaymentTransactionService _vendorPaymentTransactionService;

        public PayPalWebHookController(IVendorOrderInfoService vendorOrderInfoService, IVendorPaymentTransactionService vendorPaymentTransactionService)
        {
            _vendorOrderInfoService = vendorOrderInfoService;
            _vendorPaymentTransactionService = vendorPaymentTransactionService;
        }

        [HttpPost]
        public async Task<IActionResult> PayPalPayoutSucceeded()
        {
            using var streamReader = new StreamReader(Request.Body);
            var requestContent = await streamReader.ReadToEndAsync();
            var eventData = JsonConvert.DeserializeAnonymousType(requestContent, new
            {
                event_type = string.Empty,
                resource = new
                {
                    sender_batch_id = string.Empty,
                }
            });
            var eventType = eventData.event_type;
            var senderBatchId = eventData.resource.sender_batch_id;

            if (eventType != PayPalDefaults.WebHookEventType)
                return Ok();

            var orderInfo = await _vendorOrderInfoService.GetVendorOrderInfoByPayoutIdAsync(senderBatchId);
            if (orderInfo == null)
                return Ok();

            if (orderInfo.PaymentStatusId == (int)PaymentStatus.Paid)
                return Ok();

            var transaction = new VendorPaymentTransaction
            {
                OrderId = orderInfo.OrderId,
                VendorId = orderInfo.VendorId,
                Amount = orderInfo.VendorPayableAmount,
                CreatedOnUtc = DateTime.UtcNow,
                PaymentMethodId = (int)PaymentMethod.PayPal
            };
            await _vendorPaymentTransactionService.InsertAsync(transaction);
            orderInfo.PaymentStatusId = (int)PaymentStatus.Paid;
            await _vendorOrderInfoService.UpdateVendorOrderInfoAsync(orderInfo);
            return Ok();
        }
    }
}
