using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NopStation.Plugin.Widgets.VendorCommission.Domain;

namespace NopStation.Plugin.Payout.Stripe.Services
{
    public class VendorStripePayoutService : IVendorStripePayoutService
    {
        private readonly IVendorStripeConfigurationService _vendorStripeConfigurationService;
        private readonly StripePayoutSettings _stripePayoutSettings;

        public VendorStripePayoutService(IVendorStripeConfigurationService vendorStripeConfigurationService,
            StripePayoutSettings stripePayoutSettings)
        {
            _vendorStripeConfigurationService = vendorStripeConfigurationService;
            _stripePayoutSettings = stripePayoutSettings;
        }
        public async Task<ProcessPayoutResult> ProcessVendorPayoutAsync(ProcessPayoutRequest request)
        {
            if (request == null || request.PaymentOptionSystemName == null || request.VendorId == 0)
                throw new ArgumentNullException(nameof(request));

            var vendorStripeConfiguration = await _vendorStripeConfigurationService.GetVendorStripeConfigurationByVendorIdAsync(request.VendorId);
            if (vendorStripeConfiguration == null || string.IsNullOrEmpty(vendorStripeConfiguration.AccountId))
            {
                return new ProcessPayoutResult
                {
                    StatusCode = 400,
                    Error = "No Recipient Found"
                };
            }
            var accountId = vendorStripeConfiguration.AccountId;
            var apiSecretKey = _stripePayoutSettings.SecretKey;
            var totalAmount = (int)(request.PayoutTotal * 100);
            var transferParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("amount", totalAmount.ToString()),
                new KeyValuePair<string, string>("currency", "usd"),
                new KeyValuePair<string, string>("destination", accountId),
            };

            var content = new FormUrlEncodedContent(transferParams);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiSecretKey}");
                var response = await client.PostAsync(StripeDefaults.CreateTransfer, content);
                if (!response.IsSuccessStatusCode)
                {
                    return new ProcessPayoutResult
                    {
                        StatusCode = (int)response.StatusCode,
                        Error = await response.Content.ReadAsStringAsync()
                    };
                }
                client.DefaultRequestHeaders.Add("Stripe-Account", accountId);
                var payoutParams = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("amount", totalAmount.ToString()),
                    new KeyValuePair<string, string>("currency", "usd"),
                };
                content = new FormUrlEncodedContent(payoutParams);
                response = await client.PostAsync(StripeDefaults.CreatePayout, content);
                if (!response.IsSuccessStatusCode)
                {
                    return new ProcessPayoutResult
                    {
                        StatusCode = (int)response.StatusCode,
                        Error = await response.Content.ReadAsStringAsync()
                    };
                }
                else
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(jsonResponse);
                    var payoutId = (string)json["id"];
                    return new ProcessPayoutResult
                    {
                        StatusCode = (int)HttpStatusCode.Created,
                        SenderBatchId = payoutId
                    };
                }
            }
        }
    }
}