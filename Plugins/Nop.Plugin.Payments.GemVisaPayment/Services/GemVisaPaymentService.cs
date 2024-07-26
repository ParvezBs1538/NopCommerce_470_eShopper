using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.GemVisaPayment.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Tax;

namespace Nop.Plugin.Payments.GemVisaPayment.Services
{
    public class GemVisaPaymentService : IGemVisaPaymentService
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILogger _logger;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;

        public GemVisaPaymentService(IStoreContext storeContext,
            ISettingService settingService,
            IOrderService orderService,
            IProductService productService,
            IWorkContext workContext,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IUrlRecordService urlRecordService,
            ILogger logger,
            ICurrencyService currencyService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _orderService = orderService;
            _productService = productService;
            _workContext = workContext;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _urlRecordService = urlRecordService;
            _logger = logger;
            _currencyService = currencyService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
        }

        public PurchaseRequestBody GenerateRequestBody(Order order)
        {
            var storeScope = _storeContext.CurrentStore.Id;
            var gemSettings = _settingService.LoadSetting<GemVisaPaymentSettings>(storeScope);
            var shippingAddress = order.ShippingAddressId.HasValue ? _addressService.GetAddressById(order.ShippingAddressId.Value) : null;
            var billingAddress = _addressService.GetAddressById(order.BillingAddressId);
            var firstName = billingAddress.FirstName;
            var lastName = billingAddress.LastName;
            var billingAddressCountry = _countryService.GetCountryById(billingAddress?.CountryId ?? 0);
            var shippingAddressCountry = _countryService.GetCountryById(shippingAddress?.CountryId ?? 0);
            var gemBillingAddress = new GemAddress
            {
                Name = $"{firstName} {lastName}",
                Line1 = billingAddress?.Address1 ?? string.Empty,
                Line2 = billingAddress?.Address2 ?? string.Empty,
                City = billingAddress?.City ?? string.Empty,
                Phone = billingAddress?.PhoneNumber ?? string.Empty,
                State = _stateProvinceService.GetStateProvinceByAddress(billingAddress) is StateProvince billingStateProvince ? _localizationService.GetLocalized(billingStateProvince, x => x.Name) : null,
                PostCode = billingAddress?.ZipPostalCode ?? string.Empty,
                CountryCode = billingAddressCountry?.TwoLetterIsoCode ?? string.Empty
            };
            var gemShippingAddress = gemBillingAddress;
            if (shippingAddress != null)
            {
                gemShippingAddress.Name = $"{firstName} {lastName}";
                gemShippingAddress.Line1 = shippingAddress.Address1;
                gemShippingAddress.Line2 = shippingAddress.Address2;
                gemShippingAddress.City = shippingAddress.City;
                gemShippingAddress.Phone = shippingAddress.PhoneNumber;
                gemShippingAddress.State = _stateProvinceService.GetStateProvinceByAddress(shippingAddress) is StateProvince shippingStateProvince ? _localizationService.GetLocalized(shippingStateProvince, x => x.Name) : null;
                gemShippingAddress.PostCode = shippingAddress.ZipPostalCode;
                gemShippingAddress.CountryCode = shippingAddressCountry?.TwoLetterIsoCode;
            }
            var orderLines = new List<OrderLine>();
            var items = _orderService.GetOrderItems(order.Id);
            foreach (var item in items)
            {
                var product = _productService.GetProductById(item.ProductId);
                if (product == null)
                    continue;
                var orderLine = new OrderLine
                {
                    Name = product.Name,
                    Sku = product.Sku,
                    Quantity = item.Quantity,
                    ProductUrl = $"{_webHelper.GetStoreLocation()}{_urlRecordService.GetSeName(product)}",
                    UnitPrice = Convert.ToDouble(_priceCalculationService.RoundPrice(GetProductPrice(product))),
                    IsGiftCard = false,
                    RequiresShipping = false,
                    Amount = item.Quantity * Convert.ToDouble(_priceCalculationService.RoundPrice(GetProductPrice(product)))
                };
                orderLines.Add(orderLine);
            }

            var purchasePaymentBody = new PurchaseRequestBody
            {
                MerchantId = gemSettings.MerchantId,
                IsTest = gemSettings.UseSandbox,
                MerchantReference = order.CustomOrderNumber, //order.Id.ToString(),
                //TODO Ask
                //PromotionReference= "2012",
                PlatformType = GemVisaPaymentDefaults.PLATFORM_TYPE_DIRECT,
                PlatformVersion = "0.0.1",
                Amount = Convert.ToDouble(_priceCalculationService.RoundPrice(order.OrderTotal)),
                BillingAddress = gemBillingAddress,
                Currency = _workContext.WorkingCurrency.CurrencyCode,
                Customer = new Customer
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = billingAddress.Email,
                    Phone = billingAddress.PhoneNumber,
                },
                TotalShippingAmount = Convert.ToDouble(_priceCalculationService.RoundPrice(order.OrderShippingInclTax)),
                ShippingAddress = gemShippingAddress,
                MerchantUrls = new MerchantUrls
                {
                    //Cancel= $"{_webHelper.GetStoreLocation()}gemvisa/rejectpayment",
                    Cancel = $"{_webHelper.GetStoreLocation()}orderdetails/{order.Id}",
                    Complete = $"{_webHelper.GetStoreLocation()}gemvisa/postpaymenthandler"
                },
                OrderLines = orderLines,
                RequiresShipping = order.ShippingRateComputationMethodSystemName != "Pickup.PickupInStore",
            };
            return purchasePaymentBody;

        }

        public PurchaseResponseBody GetResponse(PurchaseRequestBody purchaseRequestBody)
        {
            var storeScope = _storeContext.CurrentStore.Id;
            var gemSettings = _settingService.LoadSetting<GemVisaPaymentSettings>(storeScope);
            var baseUrl = gemSettings.UseSandbox ? GemVisaPaymentDefaults.SANDBOX_PURCHASE_REQUEST_URL : GemVisaPaymentDefaults.PURCHASE_REQUEST_URL;
            var request = WebRequest.Create(baseUrl);
            request = GemVisaPaymentHelper.AddHeaders(request, "POST", gemSettings);
            var json = JsonConvert.SerializeObject(purchaseRequestBody);
            if (gemSettings.DebugMode)
            {
                _logger.Information("//sending initiate payment request" + json);
            }
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
                var paymentUrlResponse = JsonConvert.DeserializeObject<PurchaseResponseBody>(response);
                return paymentUrlResponse;
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
            return null;
        }
        
        public VerifyResponseBody VerifyPayment(VerifyRequestBody verifyRequestBody)
        {
            var storeScope = _storeContext.CurrentStore.Id;
            var gemSettings = _settingService.LoadSetting<GemVisaPaymentSettings>(storeScope);
            var baseUrl = gemSettings.UseSandbox ? GemVisaPaymentDefaults.SANDBOX_VERIFY_URL : GemVisaPaymentDefaults.VERIFY_URL;
            var request = WebRequest.Create(baseUrl);
            request = GemVisaPaymentHelper.AddHeaders(request, "POST", gemSettings);
            var json = JsonConvert.SerializeObject(verifyRequestBody);
            if (gemSettings.DebugMode)
            {
                _logger.Information("//sending verify request" + json);
            }
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
                var verifyResponse = JsonConvert.DeserializeObject<VerifyResponseBody>(response);
                return verifyResponse;
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
            return null;
        }

        protected decimal GetProductPrice(Product product)
        {
            var finalPriceWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer), out _);
            var finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
            return finalPriceWithDiscount;
        }
    }
}
