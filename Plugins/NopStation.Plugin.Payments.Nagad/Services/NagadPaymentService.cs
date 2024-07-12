using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Directory;
using Nop.Services.Logging;
using NopStation.Plugin.Payments.Nagad.Models;
using NopStation.Plugin.Payments.Nagad.Models.Request;
using NopStation.Plugin.Payments.Nagad.Models.Response;

namespace NopStation.Plugin.Payments.Nagad.Services
{
    public class NagadPaymentService : INagadPaymentService
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly NagadPaymentSettings _nagadPaymentSettings;
        private readonly IRepository<Order> _orderRepository;

        #endregion

        #region Ctor

        public NagadPaymentService(IWebHelper webHelper,
            ILogger logger,
            IWorkContext workContext,
            ICurrencyService currencyService,
            NagadPaymentSettings nagadPaymentSettings,
            IRepository<Order> orderRepository)
        {
            _webHelper = webHelper;
            _logger = logger;
            _workContext = workContext;
            _currencyService = currencyService;
            _nagadPaymentSettings = nagadPaymentSettings;
            _orderRepository = orderRepository;
        }

        #endregion

        #region Utilities

        private async Task<string> NagadRequestAsync(HttpMethod method, string path, string orderGuid, string errorMessage, string jsonPayload = null)
        {
            var url = (_nagadPaymentSettings.UseSandbox ? NagadPaymentDefaults.SANDBOX_BASE_URL : NagadPaymentDefaults.BASE_URL) + path;
            var clientIp = _webHelper.GetCurrentIpAddress();

            var client = new HttpClient();
            var request = new HttpRequestMessage(method, url);

            request.Headers.Add("X-KM-Api-Version", "v-0.2.0");
            request.Headers.Add("X-KM-Client-Type", "PC_WEB");
            request.Headers.Add("X-KM-IP-V4", clientIp);

            if (jsonPayload != null)
            {
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var stringResponseContent = await response.Content.ReadAsStringAsync();
                    return stringResponseContent;
                }
                var errorResponseContent = await response.Content.ReadAsStringAsync();
                var desirializeErrorResponse = System.Text.Json.JsonSerializer.Deserialize<ErrorResponseModel>(errorResponseContent);
                _logger.Error(string.Format(errorMessage, orderGuid, desirializeErrorResponse.Reason, desirializeErrorResponse.Message));
                return default;
            }
            catch (Exception ex)
            {
                _logger.Error("Nagad API request failed:", ex);
                return default;
            }
        }

        #endregion

        #region Methods

        public async Task<PaymentInitSensitiveResponseModel> NagadPaymentInitializationAsync(Order order)
        {
            try
            {
                var merchantId = _nagadPaymentSettings.MerchantId;
                var orderId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
                var paymentDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                var paymentInitPath = string.Format(NagadPaymentDefaults.CHECKOUT_INITIALIZE_PATH, merchantId, orderId);

                var sensitiveData = new PaymentInitSensitiveDataModel
                {
                    MerchantId = merchantId,
                    DateTime = paymentDateTime,
                    OrderId = orderId,
                    Challenge = Guid.NewGuid().ToString()
                };
                var paymentInitSensitiveDataModelString = JsonConvert.SerializeObject(sensitiveData);

                var encryptedSensitiveData = NagadCryptoExtension.RsaEncrypt(paymentInitSensitiveDataModelString, _nagadPaymentSettings.NPGPublicKey);
                var signature = NagadCryptoExtension.GenerateDigitalSignature(paymentInitSensitiveDataModelString, _nagadPaymentSettings.MSPrivateKey);

                if (string.IsNullOrEmpty(encryptedSensitiveData) || string.IsNullOrEmpty(signature))
                    return null;

                var paymentInitRequestModel = new PaymentInitializeRequestModel
                {
                    DateTime = paymentDateTime,
                    SensitiveData = encryptedSensitiveData,
                    Signature = signature
                };

                var serializedRequestData = JsonConvert.SerializeObject(paymentInitRequestModel);

                var response = await NagadRequestAsync(HttpMethod.Post, paymentInitPath, order.OrderGuid.ToString(), NagadPaymentDefaults.PAYMET_INITIALIZATION_FAILED, serializedRequestData);
                if (string.IsNullOrEmpty(response))
                    return null;
                var paymentInitResponse = JsonConvert.DeserializeObject<PaymentInitializeResponseModel>(response);
                var paymentInitSensitiveResponseString = NagadCryptoExtension.RsaDecrypt(paymentInitResponse.SensitiveData, _nagadPaymentSettings.MSPrivateKey);
                var paymentInitSensitiveResponse = JsonConvert.DeserializeObject<PaymentInitSensitiveResponseModel>(paymentInitSensitiveResponseString);
                return paymentInitSensitiveResponse;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Nagad Payment Initialization Failed: {ex.Message}", ex);
                return null;
            }
        }

        public async Task<string> NagadPaymentOrderCompleteAsync(Order order, PaymentInitSensitiveResponseModel initResponse)
        {
            try
            {
                if (initResponse == null)
                    return null;
                var paymentDetails = await VerifyPaymentAsync(initResponse.PaymentReferenceId, order.OrderGuid.ToString());

                if (paymentDetails == null)
                    return null;

                var paymentDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                var storeLocation = _webHelper.GetStoreLocation();
                var orderCompletePath = string.Format(NagadPaymentDefaults.CHECKOUT_COMPLETE_PATH, initResponse.PaymentReferenceId);
                var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _workContext.GetWorkingCurrencyAsync());

                var sensitiveData = new OrderCompleteSensitiveDataModel
                {
                    MerchantId = paymentDetails.MerchantId,
                    OrderId = paymentDetails.OrderId,
                    Amount = orderTotal.ToString("F2"),
                    CurrencyCode = "050",
                    Challenge = initResponse.Challenge,
                };
                var orderCompleteSensitiveDataModelString = JsonConvert.SerializeObject(sensitiveData);

                var encryptedSensitiveData = NagadCryptoExtension.RsaEncrypt(orderCompleteSensitiveDataModelString, _nagadPaymentSettings.NPGPublicKey);
                var signature = NagadCryptoExtension.GenerateDigitalSignature(orderCompleteSensitiveDataModelString, _nagadPaymentSettings.MSPrivateKey);

                if (encryptedSensitiveData == null || signature == null)
                    return null;

                var orderCompleteRequestModel = new OrderCompleteRequestModel
                {
                    SensitiveData = encryptedSensitiveData,
                    Signature = signature,
                    MerchantCallbackURL = $"{storeLocation}nagadpayment/customerreturn"
                };
                var serializedOrderCompleteRequestData = JsonConvert.SerializeObject(orderCompleteRequestModel);

                var response = await NagadRequestAsync(HttpMethod.Post, orderCompletePath, order.OrderGuid.ToString(), NagadPaymentDefaults.PAYMET_ORDER_COMPLETE_FAILED, serializedOrderCompleteRequestData);
                if (string.IsNullOrEmpty(response))
                    return null;

                var orderCompleteResponse = JsonConvert.DeserializeObject<OrderCompleteResponseModel>(response);

                return orderCompleteResponse.CallBackUrl;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Nagad Payment redirect Failed: {ex.Message}", ex);
                return null;
            }
        }

        public async Task<PaymentDetails> VerifyPaymentAsync(string referenceId, string orderGuid)
        {
            var path = string.Format(NagadPaymentDefaults.PAYMET_VERIFICATION_PATH, referenceId);
            var response = await NagadRequestAsync(HttpMethod.Get, path, orderGuid, NagadPaymentDefaults.PAYMET_VERIFICATION_FAILED);
            if (string.IsNullOrEmpty(response))
                return null;

            var paymentDetails = JsonConvert.DeserializeObject<PaymentDetails>(response);

            return paymentDetails;
        }

        public async Task<Order> GetOrderByAuthorizationTransactionId(string referenceId)
        {
            var ordersQuery = _orderRepository.Table;
            ordersQuery = ordersQuery.Where(o => referenceId == o.AuthorizationTransactionId);
            return await ordersQuery.FirstOrDefaultAsync();
        }

        #endregion
    }
}
