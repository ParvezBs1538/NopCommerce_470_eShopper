using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using NopStation.Plugin.Widgets.VendorCommission.Domain;
using NopStation.Plugin.Widgets.VendorCommission.Services;

namespace NopStation.Plugin.Payout.Stripe.Controllers
{
    public class PayoutWebhookController : BasePublicController
    {
        private readonly ILogger _logger;
        private readonly IVendorOrderInfoService _vendorOrderInfoService;
        private readonly IVendorPaymentTransactionService _vendorPaymentTransactionService;

        public PayoutWebhookController(ILogger logger,
            IVendorOrderInfoService vendorOrderInfoService,
            IVendorPaymentTransactionService vendorPaymentTransactionService)
        {
            _logger = logger;
            _vendorOrderInfoService = vendorOrderInfoService;
            _vendorPaymentTransactionService = vendorPaymentTransactionService;
        }
        [HttpPost]
        public async Task<IActionResult> PayoutCompleted()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var eventData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (eventData.TryGetValue("type", out var eventType) && eventType.ToString() == "payout.paid")
                {
                    if (eventData.TryGetValue("data", out var eventDataObject) && eventDataObject is JObject eventDataJson)
                    {
                        if (eventDataJson.TryGetValue("object", out var payoutObject) && payoutObject is JObject payoutJson)
                        {
                            if (payoutJson.TryGetValue("id", out var payoutIdValue))
                            {
                                var payoutId = payoutIdValue.ToString();
                                var orderInfo = await _vendorOrderInfoService.GetVendorOrderInfoByPayoutIdAsync(payoutId);
                                if (orderInfo != null && orderInfo.PaymentStatusId != (int)PaymentStatus.Paid)
                                {
                                    orderInfo.PaymentStatusId = (int)PaymentStatus.Paid;
                                    await _vendorOrderInfoService.UpdateVendorOrderInfoAsync(orderInfo);
                                    var transaction = new VendorPaymentTransaction
                                    {
                                        OrderId = orderInfo.OrderId,
                                        VendorId = orderInfo.VendorId,
                                        Amount = orderInfo.VendorPayableAmount,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        PaymentMethodId = (int)PaymentMethod.Stripe
                                    };
                                    await _vendorPaymentTransactionService.InsertAsync(transaction);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, ex.Message);
            }
            return Ok();
        }
    }
}
