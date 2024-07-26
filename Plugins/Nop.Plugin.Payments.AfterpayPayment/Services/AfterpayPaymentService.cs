using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.AfterpayPayment.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Tasks;
using Nop.Services.Tax;

namespace Nop.Plugin.Payments.AfterpayPayment.Services
{
    public class AfterpayPaymentService : IAfterpayPaymentService
    {
        private readonly IOrderProcessingService _orderProcessingService;
        #region Fields
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressService _addressService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ITaxService _taxService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWorkContext _workContext;
        private readonly IDiscountService _discountService;
        private readonly WidgetSettings _widgetSettings;
        private readonly IUrlRecordService _urlRecordService;
        #endregion

        #region Ctor
        public AfterpayPaymentService(
            IOrderProcessingService orderProcessingService,
            IHttpContextAccessor httpContextAccessor,
            IOrderService orderService,
            IStoreContext storeContext,
            ISettingService settingService,
            ILogger logger,
            IWebHelper webHelper,
            IUrlRecordService urlRecordService,
            CurrencySettings currencySettings,
            IAddressService addressService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            IProductService productService,
            IStateProvinceService stateProvinceService,
            ITaxService taxService,
            IScheduleTaskService scheduleTaskService,
            IWorkContext workContext,
            IDiscountService discountService,
            WidgetSettings widgetSettings)
        {
            _logger = logger;
            _orderService = orderService;
            _storeContext = storeContext;
            _settingService = settingService;
            _webHelper = webHelper;
            _urlRecordService = urlRecordService;
            _currencySettings = currencySettings;
            _addressService = addressService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _productService = productService;
            _stateProvinceService = stateProvinceService;
            _taxService = taxService;
            _scheduleTaskService = scheduleTaskService;
            _workContext = workContext;
            _discountService = discountService;
            _widgetSettings = widgetSettings;
        }
        #endregion

        public string CancelPayment(string orderToken)
        {
            var storeScope = _storeContext.CurrentStore.Id;
            var settings = _settingService.LoadSetting<AfterpayPaymentSettings>(storeScope);

            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = string.Format(AfterpayPaymentDefaults.GET_CHECKOUT, orderToken);
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = AfterpayPaymentHelper.AddHeaders(request, "GET", settings, storeLocation);

            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var capturedResponse = JsonConvert.DeserializeObject<AuthResponse>(response);
                if (capturedResponse.MerchantReference != null)
                {
                    var orderId = Convert.ToInt32(capturedResponse.MerchantReference);
                    var orderDetailsUrl = string.Format(AfterpayPaymentDefaults.ORDER_DETAILS_URL, _webHelper.GetStoreLocation(), orderId);
                    return orderDetailsUrl;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
            return (string.Empty);
        }

        public PaymentUrlResponse GetResponse(PaymentUrlRequest paymentUrlRequest)
        {
            var storeScope = _storeContext.CurrentStore.Id;
            var settings = _settingService.LoadSetting<AfterpayPaymentSettings>(storeScope);
            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = AfterpayPaymentDefaults.CHECKOUT_URL_RESOURCE;

            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = AfterpayPaymentHelper.AddHeaders(request, "POST", settings, storeLocation);

            var json = JsonConvert.SerializeObject(paymentUrlRequest);

            if (settings.Debug)
            {
                _logger.Information(json);
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
                _logger.Error(e.Message);
            }
            return null;
        }

        public AuthResponse GetPaymentStatus(string orderToken)
        {
            var storeScope = _storeContext.CurrentStore.Id;
            var settings = _settingService.LoadSetting<AfterpayPaymentSettings>(storeScope);

            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = AfterpayPaymentDefaults.FULL_CAPTURE_URL_RESOURCE;
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = AfterpayPaymentHelper.AddHeaders(request, "POST", settings, storeLocation);

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
                _logger.Error(e.Message);
            }
            return null;
        }

        public PaymentUrlRequest GeneratePaymentUrlRequest(Order order)
        {
            var shippingAddress = order.ShippingAddressId.HasValue ? _addressService.GetAddressById(order.ShippingAddressId.Value) : null;
            var billingAddress = _addressService.GetAddressById(order.BillingAddressId);
            var firstName = billingAddress.FirstName;
            var lastName = billingAddress.LastName;
            var billingAddressCountry = billingAddress.CountryId.HasValue ? _countryService.GetCountryById(billingAddress.CountryId.Value) : null;
            var afterpayBillingAddress = new AfterpayAddress
            {
                Name = $"{firstName} {lastName}",
                PhoneNumber = billingAddress.PhoneNumber,
                Line1 = billingAddress.Address1,
                Line2 = billingAddress.Address2,
                Area1 = billingAddress.City,
                Area2 = "",
                Region = _stateProvinceService.GetStateProvinceByAddress(billingAddress) is StateProvince billingStateProvince ? _localizationService.GetLocalized(billingStateProvince, x => x.Name) : null,
                PostCode = billingAddress.ZipPostalCode,
                CountryCode = billingAddressCountry?.TwoLetterIsoCode
            };

            var lineItems = new List<Item>();
            var orderItems = _orderService.GetOrderItems(order.Id);
            foreach (var orderItem in orderItems)
            {
                var product = _productService.GetProductById(orderItem.ProductId);
                var item = new Item
                {
                    Name = product.Name,
                    Sku = product.Sku,
                    Quantity = orderItem.Quantity,
                    Price = new Amount
                    {
                        PaymentAmount = orderItem.UnitPriceExclTax.ToString("0.00"),
                        Currency = _workContext.WorkingCurrency.CurrencyCode,
                    },
                    PageUrl = $"{_webHelper.GetStoreLocation()}{_urlRecordService.GetSeName(product)}"
                };
                lineItems.Add(item);
            }

            var discounts = new List<Discount>();

            var paymentUrlRequest = new PaymentUrlRequest
            {
                PaymentTotalAmount = new Amount
                {
                    PaymentAmount = order.OrderTotal.ToString("0.00"),
                    Currency = _workContext.WorkingCurrency.CurrencyCode
                },
                Merchant = new Merchant
                {
                    RedirectCancelUrl = $"{_webHelper.GetStoreLocation()}checkout/cancel",
                    RedirectConfirmUrl = $"{_webHelper.GetStoreLocation()}postpaymenthandler"
                },
                Consumer = new Consumer
                {
                    GivenNames = firstName,
                    SurName = lastName,
                    Email = billingAddress.Email,
                    PhoneNumber = billingAddress.PhoneNumber
                },
                Billing = afterpayBillingAddress,
                Shipping = shippingAddress == null ? afterpayBillingAddress : new AfterpayAddress
                {
                    Name = $"{firstName} {lastName}",
                    PhoneNumber = shippingAddress.PhoneNumber,
                    Line1 = shippingAddress.Address1,
                    Line2 = shippingAddress.Address2,
                    Area1 = shippingAddress.City,
                    Area2 = "",
                    Region = _stateProvinceService.GetStateProvinceByAddress(shippingAddress) is StateProvince shippingStateProvince ? _localizationService.GetLocalized(shippingStateProvince, x => x.Name) : null,
                    PostCode = shippingAddress.ZipPostalCode,
                    CountryCode = shippingAddress.CountryId.HasValue ? _countryService.GetCountryById(shippingAddress.CountryId.Value).TwoLetterIsoCode : ""
                },
                ShippingAmount = new Amount
                {
                    PaymentAmount = order.OrderShippingInclTax.ToString("0.00"),
                    Currency = _workContext.WorkingCurrency.CurrencyCode
                },
                TaxAmount = new Amount
                {
                    PaymentAmount = order.OrderTax.ToString("0.00"),
                    Currency = _workContext.WorkingCurrency.CurrencyCode
                },
                Items = lineItems,
                MerchantReference = order.Id.ToString(),
                Courier = null,
                Discounts = discounts
            };
            if (order.PickupInStore)
            {
                var pickupAddress = order.PickupAddressId.HasValue ? _addressService.GetAddressById(order.PickupAddressId.Value) : null;
                paymentUrlRequest.Shipping = pickupAddress == null ? null : new AfterpayAddress
                {
                    Name = $"{firstName} {lastName}",
                    PhoneNumber = pickupAddress.PhoneNumber,
                    Line1 = pickupAddress.Address1,
                    Line2 = pickupAddress.Address2,
                    Area1 = pickupAddress.City,
                    Area2 = "",
                    Region = _stateProvinceService.GetStateProvinceByAddress(pickupAddress) is StateProvince pickupStateProvince ? _localizationService.GetLocalized(pickupStateProvince, x => x.Name) : null,
                    PostCode = pickupAddress.ZipPostalCode,
                    CountryCode = pickupAddress.CountryId.HasValue ? _countryService.GetCountryById(pickupAddress.CountryId.Value).TwoLetterIsoCode : ""
                };
            }
            return paymentUrlRequest;
        }

        public AuthResponse GetCapturedResponse(Order order)
        {
            var storeScope = _storeContext.CurrentStore.Id;
            var settings = _settingService.LoadSetting<AfterpayPaymentSettings>(storeScope);

            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = AfterpayPaymentDefaults.FULL_CAPTURE_URL_RESOURCE;
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = AfterpayPaymentHelper.AddHeaders(request, "POST", settings, storeLocation);

            var token = order.CaptureTransactionId;
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
                _logger.Error(e.Message);
            }
            return new AuthResponse();
        }
        public RefundResponse RefundPayment(string token, decimal amount, int orderId)
        {
            var afterpayOrderId = GetAfterpayOrderIdByToken(token);

            var storeScope = _storeContext.CurrentStore.Id;
            var settings = _settingService.LoadSetting<AfterpayPaymentSettings>(storeScope);
            var order = _orderService.GetOrderById(orderId);
            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = string.Format(AfterpayPaymentDefaults.GET_REFUND, afterpayOrderId);
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = AfterpayPaymentHelper.AddHeaders(request, "POST", settings, storeLocation);
            var refundRequest = new RefundRequest
            {
                TotalAmount = new Amount
                {
                    PaymentAmount = amount.ToString("0.00"),
                    Currency = order.CustomerCurrencyCode
                },
                MerchantReference = orderId.ToString(),
                RefundRequestId = Guid.NewGuid().ToString(),
            };
            if (order != null)
            {
                _genericAttributeService.SaveAttribute(order, "AfterpayRefundRequestId", refundRequest.RefundRequestId);
            }
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
            }
            return null;
        }

        public string GetAfterpayOrderIdByToken(string token)
        {
            var storeScope = _storeContext.CurrentStore.Id;
            var settings = _settingService.LoadSetting<AfterpayPaymentSettings>(storeScope);

            var baseUrl = settings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var resourceAddress = string.Format(AfterpayPaymentDefaults.GET_PAYMENT, token);
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = AfterpayPaymentHelper.AddHeaders(request, "GET", settings, storeLocation);
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
                _logger.Error(e.Message);
            }
            return string.Empty;
        }
    }
}
