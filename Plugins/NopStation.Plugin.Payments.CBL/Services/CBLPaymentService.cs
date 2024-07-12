using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Stores;
using NopStation.Plugin.Payments.CBL.Models;

namespace NopStation.Plugin.Payments.CBL.Services
{
    public class CBLPaymentService : ICBLPaymentService
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;

        private readonly CBLPaymentSettings _cBLPaymentSettings;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly ICurrencyService _currencyService;
        private readonly IRepository<Order> _orderRepository;

        #endregion

        #region Ctor

        public CBLPaymentService(ILogger logger,
            IWebHelper webHelper,
            CBLPaymentSettings cBlPaymentSettings,
            ICustomerService customerService,
            IStoreService storeService,
            ICurrencyService currencyService,
            IRepository<Order> orderRepository)
        {
            _logger = logger;
            _webHelper = webHelper;
            _cBLPaymentSettings = cBlPaymentSettings;
            _customerService = customerService;
            _storeService = storeService;
            _currencyService = currencyService;
            _orderRepository = orderRepository;
        }

        #endregion

        public async Task<PaymentUrlResponse> GetResponseAsync(PaymentUrlRequest paymentUrlRequest)
        {
            var settings = _cBLPaymentSettings;
            var baseUrl = settings.UseSandbox ? CBLPaymentDefaults.SANDBOX_BASE_URL : CBLPaymentDefaults.BASE_URL;
            var resourceAddress = CBLPaymentDefaults.TRANSACTION_URL_RESOURCE;
            var resourcePort = CBLPaymentDefaults.TRANSACTION_Port;

            var request = WebRequest.Create($"{baseUrl}{resourcePort}{resourceAddress}");
            request = request.AddHeaders("POST");

            var json = JsonConvert.SerializeObject(paymentUrlRequest);

            if (settings.Debug)
            {
                await _logger.InformationAsync(json);
            }

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
                var paymentUrlResponse = JsonConvert.DeserializeObject<PaymentUrlResponse>(response);
                if (paymentUrlResponse == null || paymentUrlResponse.ResponseCode != "100")
                {
                    return null;
                }
                return paymentUrlResponse;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
            }
            return null;
        }

        public async Task<PaymentUrlRequest> GeneratePaymentUrlRequestAsync(Order order)
        {
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var store = await _storeService.GetStoreByIdAsync(order.CustomerId);
            var description = $"{store?.Name ?? ""} | " + $"Order number: {order.CustomOrderNumber}";
            var currency = await _currencyService.GetCurrencyByCodeAsync(order.CustomerCurrencyCode);
            var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, currency);

            var paymentUrlRequest = new PaymentUrlRequest
            {
                MerchanUserName = _cBLPaymentSettings.MerchantUsername,
                MerchanPassword = _cBLPaymentSettings.MerchantPassword,
                MerchantId = _cBLPaymentSettings.MerchantId,

                OrderId = order.OrderGuid.ToString(),
                PaymentAmount = (double)orderTotal,
                Currency = "050",   //only for BDT
                Description = description,
                PhoneNo = customer?.Phone ?? "",
                ReturnUrl = $"{_webHelper.GetStoreLocation()}CBL/postpaymenthandler?",
            };
            return paymentUrlRequest;
        }

        public async Task<OrderResponse> GetOrderDetailsAsync(Order order, string transactionId, string sessionID)
        {
            var settings = _cBLPaymentSettings;
            var baseUrl = settings.UseSandbox ? CBLPaymentDefaults.SANDBOX_BASE_URL : CBLPaymentDefaults.BASE_URL;
            var resourceAddress = CBLPaymentDefaults.TRANSACTION_DETAILS_URL;
            var resourcePort = CBLPaymentDefaults.TRANSACTION_Port;

            var requestUriString = $"{baseUrl}{resourcePort}{resourceAddress}";
            var request = WebRequest.Create(requestUriString);
            var orderRequest = new OrderRequest
            {
                UserName = _cBLPaymentSettings.MerchantUsername,
                Password = _cBLPaymentSettings.MerchantPassword,
                MerchantId = _cBLPaymentSettings.MerchantId,
                SessionId = sessionID,
                SecureToken = transactionId,
                OrderId = order.AuthorizationTransactionId,
            };
            request = request.AddHeaders("POST");

            var json = JsonConvert.SerializeObject(orderRequest);

            if (settings.Debug)
            {
                await _logger.InformationAsync(json);
            }

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
                return JsonConvert.DeserializeObject<OrderResponse>(response);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
            }
            return null;
        }

        public async Task<Order> GetTransactionByTransactionCodeAsync(string authorizationTransactionCode)
        {
            return await _orderRepository.Table
                .FirstOrDefaultAsync(o => o.AuthorizationTransactionCode == authorizationTransactionCode);
        }
    }
}