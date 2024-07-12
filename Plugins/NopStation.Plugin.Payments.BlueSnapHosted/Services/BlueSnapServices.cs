using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Payments.BlueSnapHosted.Models;
using NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Payments;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Services
{
    public class BlueSnapServices : IBlueSnapServices
    {
        private readonly BlueSnapSettings _blueSnapSettings;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;

        public BlueSnapServices(IGenericAttributeService genericAttributeService,
            BlueSnapSettings blueSnapSettings,
            ILogger logger,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IStoreContext storeContext)
        {
            _blueSnapSettings = blueSnapSettings;
            _logger = logger;
            _workContext = workContext;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
        }

        #region Utilities

        public string GetAPIUrl(string apiPath)
        {
            var url = _blueSnapSettings.IsSandBox ? "https://sandbox.bluesnap.com/services/2/" : "https://ws.bluesnap.com/services/2/";
            return url + apiPath;
        }

        #endregion

        #region Methods

        public async Task<string> GetTokenAsync()
        {
            var url = GetAPIUrl("payment-fields-tokens/");

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("POST"), url);
                
                request.Headers.TryAddWithoutValidation("Content-Type", "Application/XML");
                request.Headers.TryAddWithoutValidation("Accept", "Application/XML");
                request.Headers.TryAddWithoutValidation("Authorization", _blueSnapSettings.AuthorizationKey);
                request.Headers.TryAddWithoutValidation("bluesnap-version", "3.0");
                request.Headers.TryAddWithoutValidation("accept-encoding", "gzip, deflate");

                var response = await httpClient.SendAsync(request);
                var obj = response?.Headers?.Location?.ToString().Split(new string[] { url }, StringSplitOptions.None)[1];
                
                return obj;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
                return null;
            }
        }

        public async Task<PlanResponse> CreatePlanAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var url = GetAPIUrl("recurring/plans");
            var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(processPaymentRequest.OrderTotal, await _workContext.GetWorkingCurrencyAsync());

            var postRequest = new Dictionary<string, object>();
            var period = processPaymentRequest.RecurringCyclePeriod switch
            {
                RecurringProductCyclePeriod.Days => "DAILY",
                RecurringProductCyclePeriod.Weeks => "WEEKLY",
                RecurringProductCyclePeriod.Months => "MONTHLY",
                RecurringProductCyclePeriod.Years => "ANNUALLY",
                _ => "ONCE",
            };

            postRequest.Add("chargeFrequency", period);
            postRequest.Add("name", processPaymentRequest.OrderGuid);
            postRequest.Add("currency", (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode);
            postRequest.Add("recurringChargeAmount", orderTotal);
            postRequest.Add("maxNumberOfCharges", processPaymentRequest.RecurringTotalCycles);

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("POST"), url);
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", _blueSnapSettings.AuthorizationKey);

                var json = JsonConvert.SerializeObject(postRequest);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                var obj = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<PlanResponse>(obj);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
                return null;
            }
        }

        public async Task<PaymentResponse> BlueSnapPaymentAPIAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var url = GetAPIUrl("transactions/");

            var postRequest = new Dictionary<string, object>();
            var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(processPaymentRequest.OrderTotal, await _workContext.GetWorkingCurrencyAsync());

            postRequest.Add("amount", orderTotal);
            postRequest.Add("currency", (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode);

            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var pfToken = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), BlueSnapDefaults.PfTokenAttribute, storeId: storeId);

            if (string.IsNullOrEmpty(pfToken))
                return null;

            postRequest.Add("pfToken", pfToken);

            var cardData = new CardHolderInfo
            {
                firstName = processPaymentRequest.CustomValues["FirstName"].ToString(),
                lastName = processPaymentRequest.CustomValues["LastName"].ToString()
            };
            postRequest.Add("cardHolderInfo", cardData);
            postRequest.Add("cardTransactionType", "AUTH_CAPTURE");

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("POST"), url);

                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", _blueSnapSettings.AuthorizationKey);
                var json = JsonConvert.SerializeObject(postRequest);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content = data;

                var response = await httpClient.SendAsync(request);
                var obj = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<PaymentResponse>(obj);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
                return null;
            }
        }

        public async Task<RefundResponse> BlueSnapRefundAPIAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var url = GetAPIUrl("transactions/refund/" + refundPaymentRequest.Order.AuthorizationTransactionId);

            var postRequestObject = new Dictionary<string, object>();
            if (refundPaymentRequest.IsPartialRefund)
                postRequestObject.Add("amount", (int)(refundPaymentRequest.AmountToRefund));
            else
                postRequestObject.Add("reason", "Refund Order Id: " + refundPaymentRequest.Order.Id.ToString());

            postRequestObject.Add("cancelSubscriptions", false);

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("POST"), url);
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", _blueSnapSettings.AuthorizationKey);

                var json = JsonConvert.SerializeObject(postRequestObject);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content = data;

                var response = await httpClient.SendAsync(request);
                var obj = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<RefundResponse>(obj);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
                return null;
            }
        }

        public async Task<SubscriptionResponse> BlueSnapSubscriptionAPIAsync(int planId, ProcessPaymentRequest processPaymentRequest)
        {
            var url = GetAPIUrl("recurring/subscriptions");

            var postRequest = new Dictionary<string, object>();
            var customValues = processPaymentRequest.CustomValues;

            postRequest.Add("payerInfo", new
            {
                zip = customValues["Zip"],
                firstName = customValues["FirstName"],
                lastName = customValues["LastName"]
            });
            postRequest.Add("paymentSource", new
            {
                pfToken = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), BlueSnapDefaults.PfTokenAttribute, 
                    storeId: (await _storeContext.GetCurrentStoreAsync()).Id)
            });
            postRequest.Add("planId", planId);

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("POST"), url);

                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", _blueSnapSettings.AuthorizationKey);

                var json = JsonConvert.SerializeObject(postRequest);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content = data;

                var response = await httpClient.SendAsync(request);
                var obj = await response.Content.ReadAsStringAsync();
                
                return JsonConvert.DeserializeObject<SubscriptionResponse>(obj);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
                return null;
            }
        }

        public async Task<string> BlueSnapSubscriptionTransactionIdAsync(string subscriptionId)
        {
            var url = GetAPIUrl("recurring/subscriptions/" + subscriptionId + "/charges?fulldescription=true");

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("GET"), url);
                
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", _blueSnapSettings.AuthorizationKey);

                var response = await httpClient.SendAsync(request);
                var obj = await response.Content.ReadAsStringAsync();
                var jsonValue = JObject.Parse(obj);

                return jsonValue["charges"][0]["transactionId"]?.ToString();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
                return null;
            }
        }

        public async Task<bool> MakeChargeForSubscriptionIdAsync(string subscriptionId)
        {
            var url = GetAPIUrl("recurring/subscriptions/" + subscriptionId + "/run-specific");

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("POST"), url);

                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", _blueSnapSettings.AuthorizationKey);

                var response = await httpClient.SendAsync(request);
                var obj = await response.Content.ReadAsStringAsync();

                return obj.Contains("SUCCESS");
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
                return false;
            }
        }

        public async Task<SubscriptionCharges> BlueSnapChargesForSubscriptionIdAsync(string subscriptionId)
        {
            var url = GetAPIUrl("recurring/subscriptions/" + subscriptionId + "/charges?fulldescription=true");

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("GET"), url);

                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", _blueSnapSettings.AuthorizationKey);

                var response = await httpClient.SendAsync(request);
                var obj = await response.Content.ReadAsStringAsync();
                
                return JsonConvert.DeserializeObject<SubscriptionCharges>(obj);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
                return null;
            }
        }

        public async Task<string> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var url = GetAPIUrl("recurring/subscriptions/" + cancelPaymentRequest.Order.SubscriptionTransactionId);

            var putRequest = new Dictionary<string, object>
            {
                { "status", "CANCELED" }
            };

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("PUT"), url);

                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", _blueSnapSettings.AuthorizationKey);

                var json = JsonConvert.SerializeObject(putRequest);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content = data;

                var response = await httpClient.SendAsync(request);
                var obj = await response.Content.ReadAsStringAsync();

                if (!response.StatusCode.ToString().ToLower().Equals("ok"))
                    return "BlueSnap Error: " + response.StatusCode;

                return null;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
                return ex.Message;
            }
        }

        #endregion
    }
}
