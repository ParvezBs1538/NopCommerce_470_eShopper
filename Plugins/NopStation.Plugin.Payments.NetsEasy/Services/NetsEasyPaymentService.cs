using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using NopStation.Plugin.Payments.NetsEasy.Models;
using NopStation.Plugin.Payments.NetsEasy.Models.Response;
using Consumer = NopStation.Plugin.Payments.NetsEasy.Models.Consumer;
using Country = NopStation.Plugin.Payments.NetsEasy.Models.Country;
using Order = Nop.Core.Domain.Orders.Order;
using PrivatePerson = NopStation.Plugin.Payments.NetsEasy.Models.PrivatePerson;
using ShippingAddress = NopStation.Plugin.Payments.NetsEasy.Models.ShippingAddress;

namespace NopStation.Plugin.Payments.NetsEasy.Services
{
    public class NetsEasyPaymentService : INetsEasyPaymentService
    {
        private readonly TaxSettings _taxSettings;
        private readonly IPaymentService _paymentService;
        private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
        private readonly IAddressService _addressService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICurrencyService _currencyService;
        private readonly ICountryService _countryService;
        private readonly ILogger _logger;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IStoreContext _storeContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;

        public NetsEasyPaymentService(
            TaxSettings taxSettings,
            IPaymentService paymentService,
            IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
            IAddressService addressService,
            IGenericAttributeService genericAttributeService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            ICurrencyService currencyService,
            ICountryService countryService,
            ILogger logger,
            ITaxService taxService,
            IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService,
            IStoreContext storeContext,
            IPriceCalculationService priceCalculationService,
            IWorkContext workContext,
            ISettingService settingService)
        {
            _taxSettings = taxSettings;
            _paymentService = paymentService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _addressService = addressService;
            _genericAttributeService = genericAttributeService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _currencyService = currencyService;
            _countryService = countryService;
            _logger = logger;
            _taxService = taxService;
            _webHelper = webHelper;
            _orderTotalCalculationService = orderTotalCalculationService;
            _storeContext = storeContext;
            _priceCalculationService = priceCalculationService;
            _workContext = workContext;
            _settingService = settingService;
        }
        protected HttpRequestMessage InitRequestHttpClient(NetsEasyPaymentSettings netsEasyPaymentSettings, string method, string parameter)
        {
            var baseUrl = netsEasyPaymentSettings.TestMode ? NetsEasyPaymentDefaults.TestUrl : NetsEasyPaymentDefaults.LiveUrl;

            if (!string.IsNullOrWhiteSpace(parameter))
            {
                baseUrl += parameter;
            }

            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(method),
                RequestUri = new Uri(baseUrl)
            };

            request.Headers.Add("Authorization", netsEasyPaymentSettings.SecretKey);
            request.Headers.Add("CommercePlatformTag", "NopCommerce");

            return request;
        }
        protected WebRequest InitRequest(NetsEasyPaymentSettings netsEasyPaymentSettings, string method, string parameter)
        {
            var baseUrl = netsEasyPaymentSettings.TestMode ? NetsEasyPaymentDefaults.TestUrl : NetsEasyPaymentDefaults.LiveUrl;

            if (!string.IsNullOrWhiteSpace(parameter))
            {
                baseUrl += parameter;
            }

            var request = WebRequest.Create(baseUrl);

            request.Method = method;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", netsEasyPaymentSettings.SecretKey);
            request.Headers.Add("CommercePlatformTag", "NopCommerce");

            return request;
        }

        public async Task<PublicInfoModel> CreatePaymentAsync(ProcessPaymentRequest paymentRequest, int storeId)
        {
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeId);
            var request = InitRequest(netsEasyPaymentSettings, "POST", "/v1/payments");

            var payload = await GetCreatePaymentBodyAsync(netsEasyPaymentSettings, paymentRequest);

            if (netsEasyPaymentSettings.EnableLog)
                await _logger.InsertLogAsync(LogLevel.Information, "NetsEasy payload", payload);

            request.ContentLength = payload.Length;
            await using (var webStream = await request.GetRequestStreamAsync())
            {
                await using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                await requestWriter.WriteAsync(payload);
            }

            var publicInfoModel = new PublicInfoModel
            {
                CheckoutKey = netsEasyPaymentSettings.CheckoutKey
            };

            try
            {
                var webResponse = await request.GetResponseAsync();
                await using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = await responseReader.ReadToEndAsync();
                var createPaymentResponse = JsonConvert.DeserializeObject<CreatePaymentResponse>(response);
                publicInfoModel.PaymentId = createPaymentResponse.PaymentId;

                return publicInfoModel;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, e.ToString());
            }

            return null;
        }

        public async Task<bool> UpdateOrderAsync(NetsEasyPaymentSettings netsEasyPaymentSettings, Order order, string paymentId)
        {
            var request = InitRequest(netsEasyPaymentSettings, "PUT", $"/v1/payments/{paymentId}/orderitems");

            var updateOrderModel = new UpdateOrderModel
            {
                Amount = GetIntegerAmount(order.OrderShippingInclTax),
                UpdateOrderShipping = new UpdateOrderModel.Shipping()
                {
                    CostSpecified = true
                }
            };

            var payload = JsonConvert.SerializeObject(updateOrderModel);

            request.ContentLength = payload.Length;
            await using (var webStream = await request.GetRequestStreamAsync())
            {
                await using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                await requestWriter.WriteAsync(payload);
            }

            try
            {
                var webResponse = (HttpWebResponse)await request.GetResponseAsync();
                if (webResponse.StatusCode == HttpStatusCode.NoContent)
                    return true;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, e.ToString());
            }

            return false;
        }

        protected async Task<string> GetCreatePaymentBodyAsync(NetsEasyPaymentSettings netsEasyPaymentSettings, ProcessPaymentRequest paymentRequest)
        {
            var dataString = new CreatePaymentModel();

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var workingCurrency = await _workContext.GetWorkingCurrencyAsync();

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            var (shoppingCartTotal, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);

            var convertedShoppingCartTotal = 0M;
            if (shoppingCartTotal.HasValue)
            {
                convertedShoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotal.Value, workingCurrency);
            }

            dataString.Order.Amount = GetIntegerAmount(convertedShoppingCartTotal);

            foreach (var shoppingCartItem in cart)
            {
                var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
                var productUnitPrice = await _shoppingCartService.GetUnitPriceAsync(shoppingCartItem, true);

                var shoppingCartUnitPriceWithDiscountBase = await _taxService.GetProductPriceAsync(product, productUnitPrice.unitPrice, false, customer);

                var unitPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase.price, workingCurrency);

                var taxRate = 0M;
                var itemTotalExcludingTax = unitPrice * shoppingCartItem.Quantity;

                var taxAmount = 0M;
                taxRate = await GetProductTaxRateAsync(product, customer);
                taxAmount = itemTotalExcludingTax * taxRate / 100;

                var totalIncludingTax = itemTotalExcludingTax + taxAmount;
                var item = new Item
                {
                    Reference = product.Sku,
                    Name = product.Name,
                    Quantity = shoppingCartItem.Quantity,
                    Unit = "unit",
                    UnitPrice = GetIntegerAmount(unitPrice),
                    TaxRate = GetIntegerAmount(taxRate),
                    TaxAmount = GetIntegerAmount(taxAmount),
                    NetTotalAmount = GetIntegerAmount(itemTotalExcludingTax),
                    GrossTotalAmount = GetIntegerAmount(totalIncludingTax)
                };
                dataString.Order.Items.Add(item);
            }


            var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            var (orderSubTotalDiscountAmountWithoutTax, _, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);
            var (orderSubTotalDiscountAmountWithTax, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, true);

            if (orderSubTotalDiscountAmountWithoutTax > decimal.Zero)
            {

                var orderSubTotalDiscountTaxAmount = orderSubTotalDiscountAmountWithTax - orderSubTotalDiscountAmountWithoutTax;
                var orderSubTotalTaxRate = 0M;
                if (orderSubTotalDiscountTaxAmount > 0)
                    orderSubTotalTaxRate = (100 / orderSubTotalDiscountAmountWithoutTax) * orderSubTotalDiscountTaxAmount;

                orderSubTotalDiscountTaxAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderSubTotalDiscountTaxAmount, workingCurrency);
                orderSubTotalDiscountAmountWithoutTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderSubTotalDiscountAmountWithoutTax, workingCurrency);
                orderSubTotalDiscountAmountWithTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderSubTotalDiscountAmountWithTax, workingCurrency);

                dataString.Order.Items.Add(new Item
                {
                    Reference = "SubTotalDiscount",
                    Name = "SubTotalDiscount",
                    UnitPrice = -GetIntegerAmount(orderSubTotalDiscountAmountWithoutTax),
                    Quantity = 1,
                    Unit = "unit",
                    TaxRate = GetIntegerAmount(orderSubTotalTaxRate),
                    TaxAmount = -GetIntegerAmount(orderSubTotalDiscountTaxAmount),
                    NetTotalAmount = -GetIntegerAmount(orderSubTotalDiscountAmountWithoutTax),
                    GrossTotalAmount = -GetIntegerAmount(orderSubTotalDiscountAmountWithTax)
                });
            }


            var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute, store.Id);

            if (shippingOption != null)
            {
                var (shippingTotalIncludingTax, taxRate, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, true);
                if (shippingTotalIncludingTax.HasValue)
                {
                    var (shippingTotalExludingTax, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, false);
                    var shippingTotalTaxAmount = shippingTotalIncludingTax.Value - shippingTotalExludingTax.Value;
                    shippingTotalTaxAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shippingTotalTaxAmount, workingCurrency);
                    shippingTotalExludingTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shippingTotalExludingTax.Value, workingCurrency);
                    shippingTotalIncludingTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shippingTotalIncludingTax.Value, workingCurrency);

                    dataString.Order.Items.Add(new Item
                    {
                        Reference = "Shipping",
                        Name = shippingOption.Name,
                        UnitPrice = GetIntegerAmount(shippingTotalExludingTax.Value),
                        Quantity = 1,
                        Unit = "unit",
                        TaxRate = GetIntegerAmount(taxRate),
                        TaxAmount = GetIntegerAmount(shippingTotalTaxAmount),
                        NetTotalAmount = GetIntegerAmount(shippingTotalExludingTax.Value),
                        GrossTotalAmount = GetIntegerAmount(shippingTotalIncludingTax.Value)
                    });

                }
            }

            if (discountAmount > 0)
            {
                var discount = appliedDiscounts.FirstOrDefault();
                var convertedDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(discountAmount, workingCurrency);
                dataString.Order.Items.Add(new Item
                {
                    Reference = "Discount",
                    Name = discount?.Name,
                    UnitPrice = -GetIntegerAmount(convertedDiscountAmount),
                    Quantity = 1,
                    Unit = "unit",
                    TaxRate = 0,
                    TaxAmount = 0,
                    NetTotalAmount = -GetIntegerAmount(convertedDiscountAmount),
                    GrossTotalAmount = -GetIntegerAmount(convertedDiscountAmount)
                });
            }


            if (appliedGiftCards.Count > 0)
            {
                var appliedGiftCard = appliedGiftCards.FirstOrDefault();
                var giftCardAmount = appliedGiftCard?.AmountCanBeUsed;
                if (giftCardAmount != null)
                {
                    var convertedGiftCardAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(giftCardAmount.Value, workingCurrency);
                    dataString.Order.Items.Add(new Item
                    {
                        Reference = "Gift Card",
                        Name = appliedGiftCard.GiftCard.GiftCardCouponCode,
                        UnitPrice = -GetIntegerAmount(convertedGiftCardAmount),
                        Quantity = 1,
                        Unit = "unit",
                        TaxRate = 0,
                        TaxAmount = 0,
                        NetTotalAmount = -GetIntegerAmount(convertedGiftCardAmount),
                        GrossTotalAmount = -GetIntegerAmount(convertedGiftCardAmount)
                    });
                }
            }

            if (redeemedRewardPointsAmount > 0)
            {
                var convertedRewardPointAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(redeemedRewardPointsAmount, workingCurrency);
                dataString.Order.Items.Add(new Item
                {
                    Reference = "Reward Points",
                    Name = "Redeemed Reward Points",
                    UnitPrice = -GetIntegerAmount(convertedRewardPointAmount),
                    Quantity = 1,
                    Unit = "unit",
                    TaxRate = 0,
                    TaxAmount = 0,
                    NetTotalAmount = -GetIntegerAmount(convertedRewardPointAmount),
                    GrossTotalAmount = -GetIntegerAmount(convertedRewardPointAmount)
                });
            }

            if (netsEasyPaymentSettings.AdditionalFee > 0)
            {
                var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
                var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, paymentMethodSystemName);
                var (paymentMethodAdditionalFeeWithTaxBase, _) = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, customer);
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    var paymentMethodAdditionalFeeWithTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(paymentMethodAdditionalFeeWithTaxBase, workingCurrency);
                    dataString.Order.Items.Add(new Item
                    {
                        Reference = "Additional Fee",
                        Name = "Payment Method Additional Fee",
                        UnitPrice = GetIntegerAmount(paymentMethodAdditionalFeeWithTax),
                        Quantity = 1,
                        Unit = "unit",
                        TaxRate = 0,
                        TaxAmount = 0,
                        NetTotalAmount = GetIntegerAmount(paymentMethodAdditionalFeeWithTax),
                        GrossTotalAmount = GetIntegerAmount(paymentMethodAdditionalFeeWithTax)
                    });
                }
            }

            //checkout attributes
            if (customer != null)
            {
                var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CheckoutAttributes,
                    store.Id);
                var attributes = _checkoutAttributeParser.ParseAttributeValues(checkoutAttributesXml);
                if (attributes != null)
                {
                    await foreach (var (attribute, values) in attributes)
                    {
                        await foreach (var attributeValue in values)
                        {
                            var (caExclTax, taxRate) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, false, customer);
                            var (caInclTax, _) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, true, customer);

                            //tax rates
                            var caTax = caInclTax - caExclTax;
                            var item = new Item
                            {
                                Reference = "Checkout Attribute",
                                Name = attribute.Name,
                                Quantity = 1,
                                Unit = "unit",
                                UnitPrice = GetIntegerAmount(caExclTax),
                                TaxRate = GetIntegerAmount(taxRate),
                                TaxAmount = GetIntegerAmount(caTax),
                                NetTotalAmount = GetIntegerAmount(caExclTax),
                                GrossTotalAmount = GetIntegerAmount(caInclTax)
                            };
                            dataString.Order.Items.Add(item);
                        }
                    }
                }
            }

            dataString.Order.Reference = paymentRequest.OrderGuid.ToString();
            dataString.Order.Currency = workingCurrency.CurrencyCode;

            dataString.Checkout.IntegrationType = "EmbeddedCheckout";
            dataString.Checkout.Url = _webHelper.GetStoreLocation() + netsEasyPaymentSettings.CheckoutPageUrl;
            dataString.Checkout.TermsUrl = _webHelper.GetStoreLocation() + "terms";

            if (!string.IsNullOrEmpty(netsEasyPaymentSettings.LimitedToCountryIds))
            {
                var countryIds = netsEasyPaymentSettings.LimitedToCountryIds.Split(',').Select(int.Parse).ToArray();

                var countries = await (await _countryService.GetCountriesByIdsAsync(countryIds)).Select(x => new Country
                {
                    CountryCode = x.ThreeLetterIsoCode
                }).ToListAsync();

                dataString.Checkout.Shipping.Countries.AddRange(countries);
            }

            //dataString.Checkout.Shipping.MerchantHandlesShippingCost = true;
            dataString.Checkout.MerchantHandlesConsumerData = true;

            if (customer != null && customer.BillingAddressId.HasValue)
            {
                var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
                if (billingAddress.CountryId.HasValue)
                {

                    var country = await _countryService.GetCountryByIdAsync(billingAddress.CountryId.Value);
                    dataString.Checkout.Consumer = new Consumer
                    {
                        Reference = customer.Id.ToString(),
                        Email = billingAddress.Email,
                        PrivatePerson = new PrivatePerson
                        {
                            FirstName = billingAddress.FirstName,
                            LastName = billingAddress.LastName
                        },

                        ShippingAddress = new ShippingAddress
                        {
                            Country = country.ThreeLetterIsoCode,
                            City = billingAddress.City,
                            AddressLine1 = billingAddress.Address1,
                            AddressLine2 = billingAddress.Address2,
                            PostalCode = billingAddress.ZipPostalCode
                        }
                    };
                }
            }

            if (netsEasyPaymentSettings.ShowB2B)
            {
                dataString.Checkout.ConsumerType.SupportedTypes.AddRange(new[] { "B2C", "B2B" });
                dataString.Checkout.ConsumerType.Default = "B2C";
            }
            var (recurringCyclesError, recurringCycleLength, recurringCyclePeriod, recurringTotalCycles) = await _shoppingCartService.GetRecurringCycleInfoAsync(cart);

            if (string.IsNullOrEmpty(recurringCyclesError) && recurringCycleLength > 0)
            {
                var endDate = DateTime.UtcNow;
                if (recurringCyclePeriod == RecurringProductCyclePeriod.Days)
                    endDate = endDate.AddDays(recurringTotalCycles* recurringCycleLength);

                else if (recurringCyclePeriod == RecurringProductCyclePeriod.Weeks)
                    endDate = endDate.AddDays(recurringTotalCycles * recurringCycleLength * 7);

                else if (recurringCyclePeriod == RecurringProductCyclePeriod.Months)
                    endDate = endDate.AddMonths(recurringTotalCycles * recurringCycleLength);

                else if (recurringCyclePeriod == RecurringProductCyclePeriod.Years)
                    endDate = endDate.AddYears(recurringTotalCycles * recurringCycleLength);
                dataString.Subscription = new Models.Subscription
                {
                    EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.ffffzzz"),
                    Interval = netsEasyPaymentSettings.EnsureRecurringInterval? recurringCycleLength : 0,
                };

            }

            var payload = JsonConvert.SerializeObject(dataString);

            return payload;
        }

        protected int GetIntegerAmount(decimal amount)
        {
            return Convert.ToInt32(amount * 100);
        }

        protected async Task<decimal> GetProductTaxRateAsync(Product product, Customer customer)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            //prices
            var minPossiblePrice = (await _priceCalculationService.GetFinalPriceAsync(product, customer, store: store)).finalPrice;

            if (product.HasTierPrices)
            {
                //calculate price for the maximum quantity if we have tier prices, and choose minimal
                minPossiblePrice = Math.Min(minPossiblePrice,
                    (await _priceCalculationService.GetFinalPriceAsync(product, customer, store: store, quantity: int.MaxValue)).finalPrice);
            }

            var taxRate = (await _taxService.GetProductPriceAsync(product, minPossiblePrice)).taxRate;

            return taxRate;
        }

        public async Task<Payment> RetrievePaymentAsync(string paymentId, int storeId)
        {
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeId);
            var request = InitRequest(netsEasyPaymentSettings, "GET", $"/v1/payments/{paymentId}");

            try
            {
                var webResponse = await request.GetResponseAsync();

                await using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = await responseReader.ReadToEndAsync();

                var paymentResponse = JsonConvert.DeserializeObject<RetrievePaymentModel>(response);

                if (paymentResponse != null)
                    return paymentResponse.Payment;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, e.ToString());
            }

            return null;
        }
        public async Task<NetsPaymentModel> VerifyPaymentAsync(string paymentId, int storeId)
        {
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeId);
            var request = InitRequestHttpClient(netsEasyPaymentSettings, "GET", $"/v1/payments/{paymentId}");
            var client = new HttpClient();
            try
            {
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var verifyPaymentResponse = JsonConvert.DeserializeObject<VerifyPaymentModel>(responseContent).Payment;

                    if (netsEasyPaymentSettings.EnableLog)
                    {
                        await _logger.InsertLogAsync(LogLevel.Debug, $"Verify payment response for paymentId #{paymentId}", responseContent);
                    }
                    return verifyPaymentResponse;
                }
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, e.ToString());
            }

            return null;
        }
        
        public async Task<RetrieveSubscriptionChargeModel> RetrievesubscriptionChargeAsync(string bulkId, int storeId)
        {
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeId);
            var request = InitRequest(netsEasyPaymentSettings, "GET", $"/v1/subscriptions/charges/{bulkId}");

            try
            {
                var webResponse = await request.GetResponseAsync();

                await using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = await responseReader.ReadToEndAsync();

                var subscriptionChargeResponse = JsonConvert.DeserializeObject<RetrieveSubscriptionChargeModel>(response);

                if (subscriptionChargeResponse != null)
                    return subscriptionChargeResponse;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, e.ToString());
            }

            return null;
        }

        public async Task<ChargePaymentResponseModel> ChargePaymentAsync(CapturePaymentRequest capturePaymentRequest, int storeId)
        {
            var order = capturePaymentRequest.Order;
            var orderTotal = GetIntegerAmount(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _currencyService.GetCurrencyByCodeAsync(order.CustomerCurrencyCode)));
            var chargePayment = new ChargePaymentModel
            {
                Amount = orderTotal
            };

            var payload = JsonConvert.SerializeObject(chargePayment);
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeId);
            var request = InitRequest(netsEasyPaymentSettings, "POST", $"/v1/payments/{order.AuthorizationTransactionId}/charges");

            request.ContentLength = payload.Length;

            await using (var webStream = await request.GetRequestStreamAsync())
            {
                await using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                await requestWriter.WriteAsync(payload);
            }

            try
            {
                var webResponse = await request.GetResponseAsync();
                await using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = await responseReader.ReadToEndAsync();
                var chargePaymentResponse = JsonConvert.DeserializeObject<ChargePaymentResponseModel>(response);
                return chargePaymentResponse;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, e.ToString());
            }

            return null;
        }
        public async Task<ChargeSubscriptionsResponseModel> ChargeSubscriptionAsync(ChargeSubscriptions chargeSubscriptions, int storeId)
        {
            var payload = JsonConvert.SerializeObject(chargeSubscriptions);
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeId);
            var request = InitRequest(netsEasyPaymentSettings, "POST", $"/v1/subscriptions/charges");

            request.ContentLength = payload.Length;

            await using (var webStream = await request.GetRequestStreamAsync())
            {
                await using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                await requestWriter.WriteAsync(payload);
            }

            try
            {
                var webResponse = await request.GetResponseAsync();
                await using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = await responseReader.ReadToEndAsync();
                var chargeSubscriptionResponse = JsonConvert.DeserializeObject<ChargeSubscriptionsResponseModel>(response);
                return chargeSubscriptionResponse;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, e.ToString());
            }

            return null;
        }

        public async Task<RefundPaymentResponseModel> RefundChargeAsync(RefundPaymentRequest refundPaymentRequest, int storeId)
        {
            //refund previously captured payment
            var amount = refundPaymentRequest.AmountToRefund != refundPaymentRequest.Order.OrderTotal
                ? (decimal?)refundPaymentRequest.AmountToRefund
                : null;

            var order = refundPaymentRequest.Order;

            if (!refundPaymentRequest.IsPartialRefund)
            {
                amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _currencyService.GetCurrencyByCodeAsync(order.CustomerCurrencyCode));
            }

            if (amount == null)
                return null;

            var amountToRefund = GetIntegerAmount(amount.Value);
            var refundPayment = new RefundPaymentModel
            {
                Amount = amountToRefund
            };

            var payload = JsonConvert.SerializeObject(refundPayment);
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeId);
            var request = InitRequest(netsEasyPaymentSettings, "POST", $"/v1/charges/{refundPaymentRequest.Order.CaptureTransactionId}/refunds");

            request.ContentLength = payload.Length;

            await using (var webStream = await request.GetRequestStreamAsync())
            {
                await using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                await requestWriter.WriteAsync(payload);
            }

            try
            {
                var webResponse = await request.GetResponseAsync();
                await using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = await responseReader.ReadToEndAsync();
                var refundPaymentResponse = JsonConvert.DeserializeObject<RefundPaymentResponseModel>(response);
                return refundPaymentResponse;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, e.ToString());
            }

            return null;
        }

        public async Task<bool> CancelPaymentAsync(VoidPaymentRequest voidPaymentRequest, int storeId)
        {
            var order = voidPaymentRequest.Order;
            var amount = GetIntegerAmount(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _currencyService.GetCurrencyByCodeAsync(order.CustomerCurrencyCode)));

            var cancelPayment = new CancelPaymentModel()
            {
                Amount = amount
            };

            var payload = JsonConvert.SerializeObject(cancelPayment);
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeId);
            var request = InitRequest(netsEasyPaymentSettings, "POST", $"/v1/payments/{voidPaymentRequest.Order.AuthorizationTransactionId}/cancels");

            request.ContentLength = payload.Length;

            await using (var webStream = await request.GetRequestStreamAsync())
            {
                await using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                await requestWriter.WriteAsync(payload);
            }

            try
            {
                var webResponse = (HttpWebResponse)await request.GetResponseAsync();
                if (webResponse.StatusCode == HttpStatusCode.NoContent)
                    return true;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, e.ToString());
            }

            return false;
        }
    }
}