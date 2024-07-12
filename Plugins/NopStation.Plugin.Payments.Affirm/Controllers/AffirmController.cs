using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Common;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.Affirm.Domain;
using NopStation.Plugin.Payments.Affirm.Models;
using NopStation.Plugin.Payments.Affirm.Services;

namespace NopStation.Plugin.Payments.Affirm.Controllers
{
    public class AffirmController : NopStationPublicController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly AffirmPaymentSettings _affirmPaymentSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IAffirmPaymentTransactionService _affirmPaymentTransactionService;

        private readonly string _sandboxChargeURL = "https://sandbox.affirm.com/api/v2/charges";
        private readonly string _canadaSandboxChargeURL = "https://sandbox.affirm.ca/api/v2/charges";
        private readonly string _chargeURL = "https://api.affirm.com/api/v2/charges";
        private readonly string _canadaChargeURL = "https://api.affirm.ca/api/v2/charges";

        #endregion

        #region Ctor

        public AffirmController(IStoreContext storeContext,
            AffirmPaymentSettings affirmPaymentSettings,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IAffirmPaymentTransactionService affirmPaymentTransactionService)
        {
            _storeContext = storeContext;
            _affirmPaymentSettings = affirmPaymentSettings;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _affirmPaymentTransactionService = affirmPaymentTransactionService;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> Charge(string checkoutToken)
        {
            using (var httpClient = new HttpClient())
            {
                var chargeURL = string.Empty;
                if (_affirmPaymentSettings.CountryAPIMode == CountryAPIMode.USA)
                    chargeURL = _affirmPaymentSettings.UseSandbox ? _sandboxChargeURL : _chargeURL;
                else
                    chargeURL = _affirmPaymentSettings.UseSandbox ? _canadaSandboxChargeURL : _canadaChargeURL;

                using (var request = new HttpRequestMessage(new HttpMethod("POST"), chargeURL))
                {
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_affirmPaymentSettings.PublicApiKey}:{_affirmPaymentSettings.PrivateApiKey}"));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");
                    request.Content = new StringContent("{\"checkout_token\":\"" + checkoutToken + "\"}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent responseContent = response.Content;
                        ChargeJsonModel responseModel = null;

                        using (var reader = new StreamReader(responseContent.ReadAsStreamAsync().Result))
                        {
                            var responseText = await reader.ReadToEndAsync();
                            responseModel = JsonConvert.DeserializeObject<ChargeJsonModel>(responseText);
                        }

                        if (Guid.TryParse(responseModel.OrderId, out var orderId))
                        {
                            var transaction = await _affirmPaymentTransactionService.GetTransactionByReferenceAsync(orderId);
                            if (transaction == null)
                            {
                                await SaveTransactionAsync(responseModel, orderId);
                            }
                        }

                        return Json(new
                        {
                            success = true
                        });
                    }
                }
            }

            return Json(new
            {
                success = false,
                message = "Payment failed"
            });
        }

        private async Task SaveTransactionAsync(ChargeJsonModel response, Guid orderId)
        {
            var transaction = new AffirmPaymentTransaction()
            {
                ChargeId = response.Id,
                Amount = response.Amount,
                CreatedOnUtc = response.Created,
                Currency = response.Currency,
                AuthHold = response.AuthHold,
                Payable = response.Payable,
                ExpiredOnUtc = response.Expires,
                IsVoid = response.Void,
                OrderGuid = orderId
            };

            await _affirmPaymentTransactionService.InsertTransactionAsync(transaction);
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), AffirmPaymentDefaults.OrderId, transaction.OrderGuid, (await _storeContext.GetCurrentStoreAsync()).Id);
        }

        #endregion
    }
}
