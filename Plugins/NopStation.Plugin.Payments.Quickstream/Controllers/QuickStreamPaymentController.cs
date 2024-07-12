using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.Quickstream.Models;

namespace NopStation.Plugin.Payments.Quickstream.Controllers
{
    public class QuickStreamPaymentController : NopStationPublicController
    {
        private readonly ILogger _logger;
        private readonly QuickstreamSettings _quickstreamSettings;

        public QuickStreamPaymentController(ILogger logger,
            QuickstreamSettings quickstreamSettings)
        {
            _logger = logger;
            _quickstreamSettings = quickstreamSettings;
        }

        public async Task<IActionResult> ValidateCard(string cardNumber)
        {
            var url = _quickstreamSettings.UseSandbox ? string.Format(QuickStreamDefaults.SANDBOX_QUERY_CARD_SURCHARGE_URL, _quickstreamSettings.SupplierBusinessCode)
                : string.Format(QuickStreamDefaults.QUERY_CARD_SURCHARGE_URL, _quickstreamSettings.SupplierBusinessCode);

            var request = WebRequest.Create(url);
            request = QuickStreamPaymentHelper.AddHeaders(request, "POST", _quickstreamSettings.SecretApiKey);
            var json = JsonConvert.SerializeObject(new { cardNumber = cardNumber });
            request.ContentLength = json.Length;
            using (var webStream = request.GetRequestStream())
            {
                using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                requestWriter.Write(json);
            }

            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var cardSurchargeResponseBody = JsonConvert.DeserializeObject<Card>(response);
                return Json(new { success = true, surcharge = cardSurchargeResponseBody.SurchargePercentage });
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return Json(new { success = false, message = "The credit card number you entered is in an incorrect format. Please check your card and try again", status = 404 });
            }
        }
    }
}