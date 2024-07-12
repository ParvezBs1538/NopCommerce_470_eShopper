using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.Widgets.VendorCommission.Domain;
using PayoutsSdk.Core;
using PayoutsSdk.Payouts;

namespace NopStation.Plugin.Payout.PayPal.Services
{
    public class VendorPayPalPayoutService : IVendorPayPalPayoutService
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IVendorPayPalConfigurationService _vendorPayPalConfigurationService;

        public VendorPayPalPayoutService(IStoreContext storeContext,
            ISettingService settingService,
            IWebHelper webHelper,
            IVendorPayPalConfigurationService vendorPayPalConfigurationService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _webHelper = webHelper;
            _vendorPayPalConfigurationService = vendorPayPalConfigurationService;
        }
        public async Task<ProcessPayoutResult> ProcessPayPalPayoutAsync(ProcessPayoutRequest request)
        {
            var client = await GetClient();
            var vendorPayPalEmail = await GetVendorPayPalEmail(request.VendorId);
            if (string.IsNullOrEmpty(vendorPayPalEmail))
            {
                return new ProcessPayoutResult
                {
                    StatusCode = 400,
                    Error = "No Recipient Found"
                };
            }
            var requestBody = CreatePayoutRequestBody(vendorPayPalEmail, request.PayoutTotal.ToString("F2"));
            var postRequest = new PayoutsPostRequest();
            postRequest.RequestBody(requestBody);
            var response = await client.Execute(postRequest);
            var responseStatusCode = (int)response.StatusCode;
            var result = response.Result<CreatePayoutResponse>();
            var payoutResult = new ProcessPayoutResult
            {
                StatusCode = responseStatusCode,
                SenderBatchId = result.BatchHeader.SenderBatchHeader.SenderBatchId,
                Error = result.Errors?.Message ?? string.Empty
            };
            return payoutResult;
        }

        private CreatePayoutRequest CreatePayoutRequestBody(string email, string amount)
        {
            return new CreatePayoutRequest()
            {
                SenderBatchHeader = new SenderBatchHeader()
                {
                    EmailMessage = "You received a payment. Thanks for using our service!",
                    EmailSubject = "Payout Received",
                    SenderBatchId = Guid.NewGuid().ToString(),

                },
                Items = new List<PayoutItem>
                            {
                                new PayoutItem()
                                {
                                    RecipientType = "EMAIL",

                                    Amount = new Currency()
                                    {
                                        CurrencyCode = "USD",
                                        Value = amount
                                    },
                                    Receiver = email,
                                    Note = "Thanks for using our service",
                                    RecipientWallet = "PAYPAL"
                                }
                            }
            };
        }

        private async Task<PayPalHttpClient> GetClient()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paypalPayoutSettings = await _settingService.LoadSettingAsync<PayPalPayoutSettings>(storeScope);
            var clientId = paypalPayoutSettings.ClientId;
            var secretKey = paypalPayoutSettings.SecretKey;
            var useSandbox = paypalPayoutSettings.UseSandbox;
            var webUrl = _webHelper.GetStoreLocation();
            var baseUrl = "https://api-m.paypal.com";
            var environment = useSandbox ? new SandboxEnvironment(clientId, secretKey) : new PayPalEnvironment(clientId, secretKey, baseUrl, webUrl);
            return new PayPalHttpClient(environment);
        }

        private async Task<string> GetVendorPayPalEmail(int vendorId)
        {
            var configurations = await _vendorPayPalConfigurationService.GetVendorPayPalConfigurationByVendorIdAsync(vendorId);
            return configurations?.PayPalEmail ?? string.Empty;
        }
    }
}
