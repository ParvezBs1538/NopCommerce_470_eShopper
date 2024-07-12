using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using NopStation.Plugin.Payments.SSLCommerz.Domains;
using NopStation.Plugin.Payments.SSLCommerz.Sevices.Responses;
using NopStation.Plugin.Payments.SSLCommerz.Sevices.Results;
using RestSharp;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices
{
    public class SSLCommerzManager : ISSLCommerzManager
    {
        #region Fields

        protected const string LIVE_URL = "https://securepay.sslcommerz.com/";
        protected const string SANDBOX_URL = "https://sandbox.sslcommerz.com/";
        protected const string SUBMIT_URL = "gwprocess/v4/api.php";
        protected const string VALIDATION_URL = "validator/api/merchantTransIDvalidationAPI.php";

        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly IRefundService _refundService;

        #endregion

        #region Ctor

        public SSLCommerzManager(IAddressService addressService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IOrderService orderService,
            IProductService productService,
            ICustomerService customerService,
            IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
            ICurrencyService currencyService,
            ISettingService settingService,
            ITaxService taxService,
            IWebHelper webHelper,
            ILogger logger,
            IRefundService refundService)
        {
            _customerService = customerService;
            _orderService = orderService;
            _productService = productService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _currencyService = currencyService;
            _settingService = settingService;
            _taxService = taxService;
            _webHelper = webHelper;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _logger = logger;
            _refundService = refundService;
        }

        #endregion

        #region Utilities

        protected async Task AddOrderNoteAsync(Order order, object obj, NoteType noteType, bool displayToCustomer = false)
        {
            string prefix;
            switch (noteType)
            {
                case NoteType.PaymentInit:
                    prefix = "SSLCommerz Payment Request: ";
                    break;
                case NoteType.PaymentValidation:
                    prefix = "SSLCommerz Payment Validation: ";
                    break;
                case NoteType.RefundInit:
                    prefix = "SSLCommerz Refund Request: ";
                    break;
                case NoteType.RefundValidation:
                default:
                    prefix = "SSLCommerz Refund Validation: ";
                    break;
            }

            await _orderService.InsertOrderNoteAsync(new OrderNote()
            {
                OrderId = order.Id,
                CreatedOnUtc = DateTime.UtcNow,
                Note = $"{prefix} {Environment.NewLine}{JsonConvert.SerializeObject(obj, Formatting.Indented)}",
                DisplayToCustomer = displayToCustomer
            });
        }

        protected Uri GetBaseUrl(bool sandbox)
        {
            return new Uri(sandbox ? SANDBOX_URL : LIVE_URL);
        }

        protected async Task<IDictionary<string, string>> GetParametersAsync(Order order)
        {
            var storeLocation = _webHelper.GetStoreLocation();
            var sslcommerzPaymentSettings = await _settingService.LoadSettingAsync<SSLCommerzPaymentSettings>(order.StoreId);

            var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var state = await _stateProvinceService.GetStateProvinceByAddressAsync(address);
            var country = await _countryService.GetCountryByAddressAsync(address);
            var currency = await _currencyService.GetCurrencyByCodeAsync(order.CustomerCurrencyCode);

            var parameters = new Dictionary<string, string>
            {
                ["store_id"] = sslcommerzPaymentSettings.StoreID,
                ["store_passwd"] = sslcommerzPaymentSettings.Password,
                ["business"] = sslcommerzPaymentSettings.BusinessEmail,
                ["charset"] = "utf-8",
                ["rm"] = "2",
                ["bn"] = Helper.NopCommercePartnerCode,
                ["currency_code"] = currency.CurrencyCode,
                ["currency"] = currency.CurrencyCode,
                ["invoice"] = order.Id.ToString(),
                ["custom"] = order.OrderGuid.ToString(),

                ["no_shipping"] = order.ShippingStatus == ShippingStatus.ShippingNotRequired ? "1" : "2",
                ["address_override"] = order.ShippingStatus == ShippingStatus.ShippingNotRequired ? "0" : "1",
                ["first_name"] = address.FirstName,
                ["last_name"] = address.LastName,
                ["address1"] = address.Address1,
                ["address2"] = address.Address2,
                ["city"] = address.City,
                ["state"] = state?.Abbreviation,
                ["country"] = country?.TwoLetterIsoCode,
                ["zip"] = address.ZipPostalCode,
                ["email"] = address.Email,

                ["tran_id"] = order.OrderGuid.ToString(),

                ["success_url"] = $"{storeLocation}sslcommerz/success/{order.OrderGuid}",
                ["fail_url"] = $"{storeLocation}sslcommerz/failed/{order.OrderGuid}",
                ["cancel_url"] = $"{storeLocation}sslcommerz/failed/{order.OrderGuid}",

                ["cus_name"] = address.FirstName + " " + address.LastName,
                ["cus_add1"] = address.Address1,
                ["cus_add2"] = address.Address2,
                ["cus_city"] = address.City,
                ["cus_state"] = state?.Abbreviation,
                ["cus_country"] = country?.TwoLetterIsoCode,
                ["cus_postcode"] = address.ZipPostalCode,
                ["cus_email"] = address.Email,
                ["cus_phone"] = address.PhoneNumber,
                ["emi_option"] = "0",
                ["num_of_item"] = "1",
                ["product_name"] = "None",
                ["product_category"] = "Electronic",
                ["product_profile"] = order.ShippingStatus == ShippingStatus.ShippingNotRequired ? "non-physical-goods" : "physical-goods",
            };

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired && await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0) is Address shippingAddress)
            {
                parameters.Add("shipping_method", order.ShippingMethod);
                parameters.Add("ship_name", shippingAddress.FirstName + " " + shippingAddress.LastName);
                parameters.Add("ship_add1", shippingAddress.Address1);
                parameters.Add("ship_add2", shippingAddress.Address2);
                parameters.Add("ship_city", shippingAddress.City);
                parameters.Add("ship_state", (await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress))?.Abbreviation);
                parameters.Add("ship_country", (await _countryService.GetCountryByAddressAsync(shippingAddress))?.TwoLetterIsoCode);
                parameters.Add("ship_postcode", shippingAddress.ZipPostalCode);
                parameters.Add("ship_email", shippingAddress.Email);
                parameters.Add("ship_phone", shippingAddress.PhoneNumber);
            }
            else
            {
                parameters.Add("shipping_method", "NO");
            }

            var roundedOrderTotal = Math.Round(_currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate), 2);
            parameters.Add("total_amount", roundedOrderTotal.ToString("0.00", CultureInfo.InvariantCulture));

            if (sslcommerzPaymentSettings.PassProductNamesAndTotals)
                await AddOrderItemsToParametersAsync(parameters, order);
            else
            {
                parameters.Add("cmd", "_xclick");
                parameters.Add("item_name", $"Order Number {order.CustomOrderNumber}");
                parameters.Add("amount", roundedOrderTotal.ToString("0.00", CultureInfo.InvariantCulture));
            }

            parameters = parameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

            return parameters;
        }

        protected async Task AddOrderItemsToParametersAsync(IDictionary<string, string> parameters, Order order)
        {
            parameters.Add("cmd", "_cart");
            parameters.Add("upload", "1");

            var roundedCartTotal = decimal.Zero;
            var itemCount = 1;

            //order items
            foreach (var item in await _orderService.GetOrderItemsAsync(order.Id))
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var roundedItemPrice = Math.Round(_currencyService.ConvertCurrency(item.UnitPriceExclTax, order.CurrencyRate), 2);

                parameters.Add($"item_name_{itemCount}", product.Name);
                parameters.Add($"amount_{itemCount}", roundedItemPrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", item.Quantity.ToString());

                roundedCartTotal += Math.Round(_currencyService.ConvertCurrency(item.PriceExclTax, order.CurrencyRate), 2);
                itemCount++;
            }

            //checkout attributes
            var checkoutAttributeValues = _checkoutAttributeParser.ParseAttributeValues(order.CheckoutAttributesXml);
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            await foreach (var (attribute, values) in checkoutAttributeValues)
            {
                await foreach (var attributeValue in values)
                {
                    var (attributePrice, _) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, false, customer);
                    var roundedAttributePrice = Math.Round(_currencyService.ConvertCurrency(attributePrice, order.CurrencyRate), 2);

                    if (attribute != null)
                    {
                        parameters.Add($"item_name_{itemCount}", $"Checkout attribute '{attribute.Name}'");
                        parameters.Add($"amount_{itemCount}", roundedAttributePrice.ToString("0.00", CultureInfo.InvariantCulture));
                        parameters.Add($"quantity_{itemCount}", "1");

                        roundedCartTotal += roundedAttributePrice;
                        itemCount++;
                    }
                }
            }

            //shipping fee
            var roundedShippingPrice = Math.Round(_currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate), 2);
            if (roundedShippingPrice > decimal.Zero)
            {
                parameters.Add($"item_name_{itemCount}", "Shipping fee");
                parameters.Add($"amount_{itemCount}", roundedShippingPrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                roundedCartTotal += roundedShippingPrice;
                itemCount++;
            }

            //payment method additional fee
            var roundedPaymentMethodPrice = Math.Round(_currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate), 2);
            if (roundedPaymentMethodPrice > decimal.Zero)
            {
                parameters.Add($"item_name_{itemCount}", "Payment method fee");
                parameters.Add($"amount_{itemCount}", roundedPaymentMethodPrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                roundedCartTotal += roundedPaymentMethodPrice;
                itemCount++;
            }

            //tax amount
            var roundedTaxAmount = Math.Round(_currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate), 2);
            if (roundedTaxAmount > decimal.Zero)
            {
                parameters.Add($"item_name_{itemCount}", "Tax amount");
                parameters.Add($"amount_{itemCount}", roundedTaxAmount.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                roundedCartTotal += roundedTaxAmount;
                itemCount++;
            }

            var orderTotal = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            if (roundedCartTotal > orderTotal)
            {
                var discountTotal = Math.Round(roundedCartTotal - orderTotal, 2);
                parameters.Add("discount_amount_cart", discountTotal.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else if (roundedCartTotal < orderTotal)
            {
                parameters.Add($"item_name_{itemCount}", "Round total ");
                parameters.Add($"amount_{itemCount}", roundedTaxAmount.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");
            }
        }

        #endregion

        public async Task<PaymentInitResult> GetGatewayRedirectUrlAsync(Order order)
        {
            var result = new PaymentInitResult();
            var settings = await _settingService.LoadSettingAsync<SSLCommerzPaymentSettings>(order.StoreId);

            var uri = GetBaseUrl(settings.UseSandbox).Concat(SUBMIT_URL);
            var parameters = await GetParametersAsync(order);
            var client = new RestClient(uri);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            foreach (var parameter in parameters)
                request.AddParameter(parameter.Key, parameter.Value);

            var response = client.Execute(request);
            var initResponse = JsonConvert.DeserializeObject<PaymentInitResponse>(response.Content);

            if (initResponse.Status.ToLower() != "success")
            {
                result.Errors.Add(initResponse.FailedReason);
                await _logger.ErrorAsync($"SSLCommerz Payment Process Failed: {response.Content}");
            }
            else
            {
                await AddOrderNoteAsync(order, initResponse, NoteType.PaymentInit);
                order.AuthorizationTransactionCode = initResponse.SessionKey;
                await _orderService.UpdateOrderAsync(order);
                result.RedirectUrl = initResponse.RedirectGatewayURL;
            }

            return result;
        }

        public async Task<PaymentValidationResult> ValidatePaymentAsync(Order order)
        {
            var result = new PaymentValidationResult();
            var commerzPaymentSettings = await _settingService.LoadSettingAsync<SSLCommerzPaymentSettings>(order.StoreId);

            var encodedStoreID = WebUtility.UrlEncode(commerzPaymentSettings.StoreID);
            var encodedStorePassword = WebUtility.UrlEncode(commerzPaymentSettings.Password);

            var url = GetBaseUrl(commerzPaymentSettings.UseSandbox).Concat(VALIDATION_URL).AbsoluteUri +
                "?sessionkey=" + order.AuthorizationTransactionCode +
                "&store_id=" + encodedStoreID +
                "&store_passwd=" + encodedStorePassword +
                "&v=1&format=json";

            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);

            var validationResponse = JsonConvert.DeserializeObject<PaymentValidationResponse>(response.Content);
            await AddOrderNoteAsync(order, validationResponse, NoteType.PaymentValidation);

            if (validationResponse.Status.ToLower() == "valid")
            {
                var orderTotal = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                if (orderTotal > validationResponse.CurrencyAmount)
                    result.Errors.Add("Invalid amount");
                else
                {
                    order.AuthorizationTransactionId = validationResponse.BankTransanctionId;
                    await _orderService.UpdateOrderAsync(order);
                }
            }
            else
                result.Errors.Add("Invalid transaction");

            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            var order = refundPaymentRequest.Order;
            var commerzPaymentSettings = await _settingService.LoadSettingAsync<SSLCommerzPaymentSettings>(order.StoreId);

            var encodedStoreID = WebUtility.UrlEncode(commerzPaymentSettings.StoreID);
            var encodedStorePassword = WebUtility.UrlEncode(commerzPaymentSettings.Password);
            var refundAmount = _currencyService.ConvertCurrency(refundPaymentRequest.AmountToRefund, order.CurrencyRate);

            var url = GetBaseUrl(commerzPaymentSettings.UseSandbox).Concat(VALIDATION_URL).AbsoluteUri +
                "?bank_tran_id=" + order.AuthorizationTransactionId +
                "&store_id=" + encodedStoreID +
                "&store_passwd=" + encodedStorePassword +
                "&refund_amount=" + refundAmount +
                "&refund_remarks=Out of Stock&v=1&format=json";

            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);

            var initResponse = JsonConvert.DeserializeObject<RefundInitResponse>(response.Content);
            await AddOrderNoteAsync(order, initResponse, NoteType.RefundInit);

            if (initResponse.Status.ToLower() != "success")
                result.Errors.Add(response.ErrorMessage);
            else
            {
                await _refundService.InsertRefundAsync(new Refund()
                {
                    RefundAmount = refundPaymentRequest.AmountToRefund,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id,
                    RefrenceId = initResponse.RefundRefrenceId
                });
            }

            return result;
        }

        public async Task<RefundValidationResult> VerifyRefundAsync(Refund refund, Order order)
        {
            var result = new RefundValidationResult();
            var commerzPaymentSettings = await _settingService.LoadSettingAsync<SSLCommerzPaymentSettings>(order.StoreId);

            var encodedStoreID = WebUtility.UrlEncode(commerzPaymentSettings.StoreID);
            var encodedStorePassword = WebUtility.UrlEncode(commerzPaymentSettings.Password);

            var url = GetBaseUrl(commerzPaymentSettings.UseSandbox).Concat(VALIDATION_URL).AbsoluteUri +
                "?refund_ref_id=" + refund.RefrenceId +
                "&store_id=" + encodedStoreID +
                "&store_passwd=" + encodedStorePassword +
                "&v=1&format=json";

            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);

            var validationResponse = JsonConvert.DeserializeObject<RefundValidationResponse>(response.Content);
            result.Success = validationResponse.Status.ToLower() == "refunded";
            result.InitiatedOn = validationResponse.InitiatedOn;

            await AddOrderNoteAsync(order, validationResponse, NoteType.RefundValidation);

            return result;
        }
    }
}