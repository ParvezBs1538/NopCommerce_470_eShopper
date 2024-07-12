using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Seo;
using NopStation.Plugin.Payments.Afterpay.Models;

namespace NopStation.Plugin.Payments.Afterpay.Services
{
    public class AfterpayPaymentService : IAfterpayPaymentService
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly AfterpayPaymentSettings _afterpayPaymentSettings;

        #endregion

        #region Ctor

        public AfterpayPaymentService(IOrderService orderService,
            ILogger logger,
            IWebHelper webHelper,
            IUrlRecordService urlRecordService,
            IAddressService addressService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IProductService productService,
            IStateProvinceService stateProvinceService,
            IWorkContext workContext,
            AfterpayPaymentSettings afterpayPaymentSettings)
        {
            _logger = logger;
            _orderService = orderService;
            _webHelper = webHelper;
            _urlRecordService = urlRecordService;
            _addressService = addressService;
            _countryService = countryService;
            _localizationService = localizationService;
            _productService = productService;
            _stateProvinceService = stateProvinceService;
            _workContext = workContext;
            _afterpayPaymentSettings = afterpayPaymentSettings;
        }

        #endregion

        public async Task<AuthResponse> GetCancelPaymentResponseAsync(string orderToken)
        {
            var settings = _afterpayPaymentSettings;
            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = string.Format(AfterpayPaymentDefaults.GET_CHECKOUT, orderToken);
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = request.AddHeaders("GET", settings, storeLocation);

            var webResponse = await request.GetResponseAsync();
            using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
            using var responseReader = new StreamReader(webStream);
            var response = responseReader.ReadToEnd();
            var capturedResponse = JsonConvert.DeserializeObject<AuthResponse>(response);

            return capturedResponse;
        }

        public async Task<PaymentUrlResponse> GetResponseAsync(PaymentUrlRequest paymentUrlRequest)
        {
            var settings = _afterpayPaymentSettings;
            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = AfterpayPaymentDefaults.CHECKOUT_URL_RESOURCE;

            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = request.AddHeaders("POST", settings, storeLocation);

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
                return paymentUrlResponse;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
            }
            return null;
        }

        public async Task<AuthResponse> GetPaymentStatusAsync(string orderToken)
        {
            var settings = _afterpayPaymentSettings;

            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = AfterpayPaymentDefaults.FULL_CAPTURE_URL_RESOURCE;
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = request.AddHeaders("POST", settings, storeLocation);

            var json = JsonConvert.SerializeObject(new { token = orderToken });
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
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(response);

                return authResponse;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
            }
            return null;
        }

        public async Task<PaymentUrlRequest> GeneratePaymentUrlRequestAsync(Order order)
        {
            var currency = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
            var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);
            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            var afterpayBillingAddress = new AfterpayAddress
            {
                Name = $"{billingAddress.FirstName} {billingAddress.LastName}",
                PhoneNumber = billingAddress.PhoneNumber,
                Line1 = billingAddress.Address1,
                Line2 = billingAddress.Address2,
                Area1 = billingAddress.City,
                Region = (await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress))?.Name,
                PostCode = billingAddress.ZipPostalCode,
                CountryCode = (await _countryService.GetCountryByIdAsync(billingAddress.CountryId ?? 0))?.TwoLetterIsoCode
            };

            var lineItems = new List<Item>();
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                lineItems.Add(new Item
                {
                    Name = product.Name,
                    Sku = product.Sku,
                    Quantity = orderItem.Quantity,
                    Price = new PaymentAfterpayModel
                    {
                        PaymentAmount = orderItem.UnitPriceInclTax.ToString("0.00"),
                        Currency = currency,
                    },
                    PageUrl = $"{_webHelper.GetStoreLocation()}{await _urlRecordService.GetSeNameAsync(product)}"
                });
            }

            var discounts = new List<Discount>();

            var paymentUrlRequest = new PaymentUrlRequest
            {
                PaymentTotalAmount = new PaymentAfterpayModel
                {
                    PaymentAmount = order.OrderTotal.ToString("0.00"),
                    Currency = currency
                },
                Merchant = new Merchant
                {
                    RedirectCancelUrl = $"{_webHelper.GetStoreLocation()}afterpay/cancelpayment",
                    RedirectConfirmUrl = $"{_webHelper.GetStoreLocation()}afterpay/postpaymenthandler"
                },
                Consumer = new Consumer
                {
                    GivenNames = billingAddress.FirstName,
                    SurName = billingAddress.LastName,
                    Email = billingAddress.Email,
                    PhoneNumber = billingAddress.PhoneNumber
                },
                Billing = afterpayBillingAddress,
                Shipping = shippingAddress == null ? afterpayBillingAddress : new AfterpayAddress
                {
                    Name = $"{shippingAddress.FirstName} {shippingAddress.LastName}",
                    PhoneNumber = shippingAddress.PhoneNumber,
                    Line1 = shippingAddress.Address1,
                    Line2 = shippingAddress.Address2,
                    Area1 = shippingAddress.City,
                    Area2 = "",
                    Region = await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince shippingStateProvince ? await _localizationService.GetLocalizedAsync(shippingStateProvince, x => x.Name) : null,
                    PostCode = shippingAddress.ZipPostalCode,
                    CountryCode = shippingAddress.CountryId.HasValue ? (await _countryService.GetCountryByIdAsync(shippingAddress.CountryId.Value)).TwoLetterIsoCode : ""
                },
                ShippingAmount = new PaymentAfterpayModel
                {
                    PaymentAmount = order.OrderShippingInclTax.ToString("0.00"),
                    Currency = currency
                },
                TaxAmount = new PaymentAfterpayModel
                {
                    PaymentAmount = order.OrderTax.ToString("0.00"),
                    Currency = currency
                },
                Items = lineItems,
                MerchantReference = order.Id.ToString(),
                Courier = null,
                Discounts = discounts
            };
            if (order.PickupInStore)
            {
                var pickupAddress = order.PickupAddressId.HasValue ? await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) : null;
                paymentUrlRequest.Shipping = pickupAddress == null ? null : new AfterpayAddress
                {
                    Name = $"{pickupAddress.FirstName} {pickupAddress.LastName}",
                    PhoneNumber = pickupAddress.PhoneNumber,
                    Line1 = pickupAddress.Address1,
                    Line2 = pickupAddress.Address2,
                    Area1 = pickupAddress.City,
                    Area2 = "",
                    Region = await _stateProvinceService.GetStateProvinceByAddressAsync(pickupAddress) is StateProvince pickupStateProvince ? await _localizationService.GetLocalizedAsync(pickupStateProvince, x => x.Name) : null,
                    PostCode = pickupAddress.ZipPostalCode,
                    CountryCode = pickupAddress.CountryId.HasValue ? (await _countryService.GetCountryByIdAsync(pickupAddress.CountryId.Value)).TwoLetterIsoCode : ""
                };
            }
            return paymentUrlRequest;
        }

        public async Task<AuthResponse> GetCapturedResponseAsync(Order order)
        {
            var settings = _afterpayPaymentSettings;

            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = AfterpayPaymentDefaults.FULL_CAPTURE_URL_RESOURCE;
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = request.AddHeaders("POST", settings, storeLocation);

            var token = order.AuthorizationTransactionId;
            var json = JsonConvert.SerializeObject(new { token = token });
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
                var capturedResponse = JsonConvert.DeserializeObject<AuthResponse>(response);
                return capturedResponse;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
            }
            return new AuthResponse();
        }

        public async Task<RefundResponse> RefundPaymentAsync(string token, decimal amount, int orderId)
        {
            var afterpayOrderId = await GetAfterpayOrderIdByTokenAsync(token);

            var settings = _afterpayPaymentSettings;
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var url = string.Format(AfterpayPaymentDefaults.GET_REFUND, baseUrl, afterpayOrderId);
            var request = WebRequest.Create($"{baseUrl}{url}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = request.AddHeaders("POST", settings, storeLocation);
            var refundRequest = new RefundRequest
            {
                TotalAmount = new PaymentAfterpayModel
                {
                    PaymentAmount = amount.ToString("0.00"),
                    Currency = order.CustomerCurrencyCode
                },
                MerchantReference = orderId.ToString(),
                RefundRequestId = Guid.NewGuid().ToString(),
            };

            var json = JsonConvert.SerializeObject(refundRequest);
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
                var refundResponse = JsonConvert.DeserializeObject<RefundResponse>(response);
                return refundResponse;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
            }

            return null;
        }

        public async Task<string> GetAfterpayOrderIdByTokenAsync(string token)
        {
            var settings = _afterpayPaymentSettings;
            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = string.Format(AfterpayPaymentDefaults.GET_PAYMENT, token);
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = request.AddHeaders("GET", settings, storeLocation);
            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);

                var response = responseReader.ReadToEnd();
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(response);

                return authResponse.Id;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
            }

            return string.Empty;
        }
    }
}
