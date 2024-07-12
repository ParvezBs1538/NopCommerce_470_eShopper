using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public class WebApiNotificationTokenProvider : IPushNotificationTokenProvider
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAttributeFormatter<AddressAttribute, AddressAttributeValue> _addressAttributeFormatter;
        private readonly IAttributeFormatter<CustomerAttribute, CustomerAttributeValue> _customerAttributeFormatter;
        private readonly IAttributeFormatter<VendorAttribute, VendorAttributeValue> _vendorAttributeFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly PaymentSettings _paymentSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IProductService _productService;
        private readonly IHtmlFormatter _htmlFormatter;
        private Dictionary<string, IEnumerable<string>> _allowedTokens;

        #endregion

        #region Ctor

        public WebApiNotificationTokenProvider(CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
            IAttributeFormatter<CustomerAttribute, CustomerAttributeValue> customerAttributeFormatter,
            IAttributeFormatter<VendorAttribute, VendorAttributeValue> vendorAttributeFormatter,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPriceFormatter priceFormatter,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            PaymentSettings paymentSettings,
            StoreInformationSettings storeInformationSettings,
            IAddressService addressService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IProductService productService,
            IHtmlFormatter htmlFormatter)
        {
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addressAttributeFormatter = addressAttributeFormatter;
            _customerAttributeFormatter = customerAttributeFormatter;
            _vendorAttributeFormatter = vendorAttributeFormatter;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _priceFormatter = priceFormatter;
            _storeContext = storeContext;
            _storeService = storeService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _paymentSettings = paymentSettings;
            _storeInformationSettings = storeInformationSettings;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _productService = productService;
            _htmlFormatter = htmlFormatter;
        }

        #endregion

        protected Dictionary<string, IEnumerable<string>> AllowedTokens
        {
            get
            {
                if (_allowedTokens != null)
                    return _allowedTokens;

                _allowedTokens = new Dictionary<string, IEnumerable<string>>();

                //store tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.StoreTokens, new[]
                {
                    "%Store.Name%",
                    "%Store.URL%",
                    "%Store.CompanyName%",
                    "%Store.CompanyAddress%",
                    "%Store.CompanyPhoneNumber%",
                    "%Store.CompanyVat%",
                    "%Facebook.URL%",
                    "%Twitter.URL%",
                    "%YouTube.URL%",
                    "%Instagram.URL%"
                });

                //customer tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.CustomerTokens, new[]
                {
                    "%Customer.Email%",
                    "%Customer.Username%",
                    "%Customer.FullName%",
                    "%Customer.FirstName%",
                    "%Customer.LastName%",
                    "%Customer.VatNumber%",
                    "%Customer.VatNumberStatus%",
                    "%Customer.CustomAttributes%",
                    "%Customer.PasswordRecoveryURL%",
                    "%Customer.AccountActivationURL%",
                    "%Customer.EmailRevalidationURL%",
                    "%Wishlist.URLForCustomer%"
                });

                //order tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.OrderTokens, new[]
                {
                    "%Order.OrderNumber%",
                    "%Order.CustomerFullName%",
                    "%Order.CustomerEmail%",
                    "%Order.BillingFirstName%",
                    "%Order.BillingLastName%",
                    "%Order.BillingPhoneNumber%",
                    "%Order.BillingEmail%",
                    "%Order.BillingFaxNumber%",
                    "%Order.BillingCompany%",
                    "%Order.BillingAddress1%",
                    "%Order.BillingAddress2%",
                    "%Order.BillingCity%",
                    "%Order.BillingCounty%",
                    "%Order.BillingStateProvince%",
                    "%Order.BillingZipPostalCode%",
                    "%Order.BillingCountry%",
                    "%Order.BillingCustomAttributes%",
                    "%Order.Shippable%",
                    "%Order.ShippingMethod%",
                    "%Order.ShippingFirstName%",
                    "%Order.ShippingLastName%",
                    "%Order.ShippingPhoneNumber%",
                    "%Order.ShippingEmail%",
                    "%Order.ShippingFaxNumber%",
                    "%Order.ShippingCompany%",
                    "%Order.ShippingAddress1%",
                    "%Order.ShippingAddress2%",
                    "%Order.ShippingCity%",
                    "%Order.ShippingCounty%",
                    "%Order.ShippingStateProvince%",
                    "%Order.ShippingZipPostalCode%",
                    "%Order.ShippingCountry%",
                    "%Order.ShippingCustomAttributes%",
                    "%Order.PaymentMethod%",
                    "%Order.VatNumber%",
                    "%Order.CustomValues%",
                    "%Order.CreatedOn%",
                    "%Order.OrderURLForCustomer%",
                    "%Order.PickupInStore%",
                    "%Order.OrderId%"
                });

                //shipment tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.ShipmentTokens, new[]
                {
                    "%Shipment.ShipmentNumber%",
                    "%Shipment.TrackingNumber%",
                    "%Shipment.TrackingNumberURL%",
                    "%Shipment.URLForCustomer%"
                });

                //refunded order tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.RefundedOrderTokens, new[]
                {
                    "%Order.AmountRefunded%"
                });

                //order note tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.OrderNoteTokens, new[]
                {
                    "%Order.NewNoteText%",
                    "%Order.OrderNoteAttachmentUrl%"
                });

                //recurring payment tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.RecurringPaymentTokens, new[]
                {
                    "%RecurringPayment.ID%",
                    "%RecurringPayment.CancelAfterFailedPayment%",
                    "%RecurringPayment.RecurringPaymentType%"
                });

                //newsletter subscription tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.SubscriptionTokens, new[]
                {
                    "%NewsLetterSubscription.Email%",
                    "%NewsLetterSubscription.ActivationUrl%",
                    "%NewsLetterSubscription.DeactivationUrl%"
                });

                //product tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.ProductTokens, new[]
                {
                    "%Product.ID%",
                    "%Product.Name%",
                    "%Product.ShortDescription%",
                    "%Product.SKU%",
                    "%Product.StockQuantity%"
                });

                //return request tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.ReturnRequestTokens, new[]
                {
                    "%ReturnRequest.CustomNumber%",
                    "%ReturnRequest.OrderId%",
                    "%ReturnRequest.Product.Quantity%",
                    "%ReturnRequest.Product.Name%",
                    "%ReturnRequest.Reason%",
                    "%ReturnRequest.RequestedAction%",
                    "%ReturnRequest.CustomerComment%",
                    "%ReturnRequest.StaffNotes%",
                    "%ReturnRequest.Status%"
                });

                //vendor tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.VendorTokens, new[]
                {
                    "%Vendor.Name%",
                    "%Vendor.Email%",
                    "%Vendor.VendorAttributes%"
                });

                //gift card tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.GiftCardTokens, new[]
                {
                    "%GiftCard.SenderName%",
                    "%GiftCard.SenderEmail%",
                    "%GiftCard.RecipientName%",
                    "%GiftCard.RecipientEmail%",
                    "%GiftCard.Amount%",
                    "%GiftCard.CouponCode%",
                    "%GiftCard.Message%"
                });

                //product review tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.ProductReviewTokens, new[]
                {
                    "%ProductReview.ProductName%",
                    "%ProductReview.Title%",
                    "%ProductReview.IsApproved%",
                    "%ProductReview.ReviewText%",
                    "%ProductReview.ReplyText%"
                });

                //attribute combination tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.AttributeCombinationTokens, new[]
                {
                    "%AttributeCombination.Formatted%",
                    "%AttributeCombination.SKU%",
                    "%AttributeCombination.StockQuantity%"
                });

                //product back in stock tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.ProductBackInStockTokens, new[]
                {
                    "%BackInStockSubscription.ProductName%",
                    "%BackInStockSubscription.ProductUrl%"
                });

                //VAT validation tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.VatValidation, new[]
                {
                    "%VatValidationResult.Name%",
                    "%VatValidationResult.Address%"
                });

                //contact vendor tokens
                _allowedTokens.Add(WebApiNotificationTokenGroupNames.ContactVendor, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                return _allowedTokens;
            }
        }

        public IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups)
        {
            var allowedTokens = AllowedTokens.Where(x => tokenGroups == null || tokenGroups.Contains(x.Key))
                .SelectMany(x => x.Value).ToList();

            return allowedTokens.Distinct();
        }

        public IEnumerable<string> GetTokenGroups(WebApiNotificationTemplate pushNotificationTemplate)
        {
            switch (pushNotificationTemplate?.Name)
            {
                case WebApiNotificationTemplateSystemNames.ORDER_PAID_CUSTOMER_NOTIFICATION:
                case WebApiNotificationTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION:
                case WebApiNotificationTemplateSystemNames.ORDER_COMPLETED_CUSTOMER_NOTIFICATION:
                case WebApiNotificationTemplateSystemNames.ORDER_CANCELLED_CUSTOMER_NOTIFICATION:
                    return new[] { WebApiNotificationTokenGroupNames.StoreTokens, WebApiNotificationTokenGroupNames.OrderTokens, WebApiNotificationTokenGroupNames.CustomerTokens };

                case WebApiNotificationTemplateSystemNames.SHIPMENT_SENT_CUSTOMER_NOTIFICATION:
                case WebApiNotificationTemplateSystemNames.SHIPMENT_DELIVERED_CUSTOMER_NOTIFICATION:
                    return new[] { WebApiNotificationTokenGroupNames.StoreTokens, WebApiNotificationTokenGroupNames.ShipmentTokens, WebApiNotificationTokenGroupNames.OrderTokens, WebApiNotificationTokenGroupNames.CustomerTokens };

                case WebApiNotificationTemplateSystemNames.ORDER_REFUNDED_CUSTOMER_NOTIFICATION:
                    return new[] { WebApiNotificationTokenGroupNames.StoreTokens, WebApiNotificationTokenGroupNames.OrderTokens, WebApiNotificationTokenGroupNames.RefundedOrderTokens, WebApiNotificationTokenGroupNames.CustomerTokens };

                case WebApiNotificationTemplateSystemNames.CUSTOMER_REGISTERED_NOTIFICATION:
                case WebApiNotificationTemplateSystemNames.CUSTOMER_WELCOME_NOTIFICATION:
                case WebApiNotificationTemplateSystemNames.CUSTOMER_EMAIL_VALIDATION_NOTIFICATION:
                case WebApiNotificationTemplateSystemNames.CUSTOMER_REGISTERED_WELCOME_NOTIFICATION:
                default:
                    return new[] { WebApiNotificationTokenGroupNames.StoreTokens, WebApiNotificationTokenGroupNames.CustomerTokens };
            }
        }

        public virtual async Task AddCustomerTokensAsync(IList<Token> tokens, Customer customer)
        {
            tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.Username", customer.Username));
            tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
            tokens.Add(new Token("Customer.FirstName", customer.FirstName));
            tokens.Add(new Token("Customer.LastName", customer.LastName));
            tokens.Add(new Token("Customer.VatNumber", customer.VatNumber));
            tokens.Add(new Token("Customer.VatNumberStatus", ((VatNumberStatus)customer.VatNumberStatusId).ToString()));

            var customAttributesXml = customer.CustomCustomerAttributesXML;
            tokens.Add(new Token("Customer.CustomAttributes", await _customerAttributeFormatter.FormatAttributesAsync(customAttributesXml), true));

            //note: we do not use SEO friendly URLS for these links because we can get errors caused by having .(dot) in the URL (from the email address)
            var passwordRecoveryUrl = await RouteUrlAsync(routeName: "PasswordRecoveryConfirm", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute), guid = customer.CustomerGuid });
            var accountActivationUrl = await RouteUrlAsync(routeName: "AccountActivation", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.AccountActivationTokenAttribute), guid = customer.CustomerGuid });
            var emailRevalidationUrl = await RouteUrlAsync(routeName: "EmailRevalidation", routeValues: new { token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute), guid = customer.CustomerGuid });
            var wishlistUrl = await RouteUrlAsync(routeName: "Wishlist", routeValues: new { customerGuid = customer.CustomerGuid });
            tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("Customer.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("Customer.EmailRevalidationURL", emailRevalidationUrl, true));
            tokens.Add(new Token("Wishlist.URLForCustomer", wishlistUrl, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(customer, tokens);
        }

        public virtual async Task AddStoreTokensAsync(IList<Token> tokens, Store store)
        {
            tokens.Add(new Token("Store.Name", await _localizationService.GetLocalizedAsync(store, x => x.Name)));
            tokens.Add(new Token("Store.URL", store.Url, true));
            tokens.Add(new Token("Store.CompanyName", store.CompanyName));
            tokens.Add(new Token("Store.CompanyAddress", store.CompanyAddress));
            tokens.Add(new Token("Store.CompanyPhoneNumber", store.CompanyPhoneNumber));
            tokens.Add(new Token("Store.CompanyVat", store.CompanyVat));

            tokens.Add(new Token("Facebook.URL", _storeInformationSettings.FacebookLink));
            tokens.Add(new Token("Twitter.URL", _storeInformationSettings.TwitterLink));
            tokens.Add(new Token("YouTube.URL", _storeInformationSettings.YoutubeLink));
            tokens.Add(new Token("Instagram.URL", _storeInformationSettings.InstagramLink));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(store, tokens);
        }

        public virtual async Task AddOrderTokensAsync(IList<Token> tokens, Order order, int languageId, int vendorId = 0)
        {
            //lambda expression for choosing correct order address
            async Task<Address> orderAddressAsync(Order o) => await _addressService.GetAddressByIdAsync((o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) ?? 0);
            var orderAddress = await orderAddressAsync(order);

            tokens.Add(new Token("Order.OrderId", order.Id));
            tokens.Add(new Token("Order.OrderNumber", order.CustomOrderNumber));

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            tokens.Add(new Token("Order.CustomerFullName", $"{billingAddress.FirstName} {billingAddress.LastName}"));
            tokens.Add(new Token("Order.CustomerEmail", billingAddress.Email));

            tokens.Add(new Token("Order.BillingFirstName", billingAddress.FirstName));
            tokens.Add(new Token("Order.BillingLastName", billingAddress.LastName));
            tokens.Add(new Token("Order.BillingPhoneNumber", billingAddress.PhoneNumber));
            tokens.Add(new Token("Order.BillingEmail", billingAddress.Email));
            tokens.Add(new Token("Order.BillingFaxNumber", billingAddress.FaxNumber));
            tokens.Add(new Token("Order.BillingCompany", billingAddress.Company));
            tokens.Add(new Token("Order.BillingAddress1", billingAddress.Address1));
            tokens.Add(new Token("Order.BillingAddress2", billingAddress.Address2));
            tokens.Add(new Token("Order.BillingCity", billingAddress.City));
            tokens.Add(new Token("Order.BillingCounty", billingAddress.County));
            tokens.Add(new Token("Order.BillingStateProvince", await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) is StateProvince billingStateProvince ? await _localizationService.GetLocalizedAsync(billingStateProvince, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.BillingZipPostalCode", billingAddress.ZipPostalCode));
            tokens.Add(new Token("Order.BillingCountry", await _countryService.GetCountryByAddressAsync(billingAddress) is Country billingCountry ? await _localizationService.GetLocalizedAsync(billingCountry, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.BillingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync(billingAddress.CustomAttributes), true));

            tokens.Add(new Token("Order.Shippable", !string.IsNullOrEmpty(order.ShippingMethod)));
            tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
            tokens.Add(new Token("Order.PickupInStore", order.PickupInStore));
            tokens.Add(new Token("Order.ShippingFirstName", orderAddress?.FirstName ?? string.Empty));
            tokens.Add(new Token("Order.ShippingLastName", orderAddress?.LastName ?? string.Empty));
            tokens.Add(new Token("Order.ShippingPhoneNumber", orderAddress?.PhoneNumber ?? string.Empty));
            tokens.Add(new Token("Order.ShippingEmail", orderAddress?.Email ?? string.Empty));
            tokens.Add(new Token("Order.ShippingFaxNumber", orderAddress?.FaxNumber ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCompany", orderAddress?.Company ?? string.Empty));
            tokens.Add(new Token("Order.ShippingAddress1", orderAddress?.Address1 ?? string.Empty));
            tokens.Add(new Token("Order.ShippingAddress2", orderAddress?.Address2 ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCity", orderAddress?.City ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCounty", orderAddress?.County ?? string.Empty));
            tokens.Add(new Token("Order.ShippingStateProvince", await _stateProvinceService.GetStateProvinceByAddressAsync(orderAddress) is StateProvince shippingStateProvince ? _localizationService.GetLocalizedAsync(shippingStateProvince, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.ShippingZipPostalCode", orderAddress?.ZipPostalCode ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCountry", await _countryService.GetCountryByAddressAsync(orderAddress) is Country orderCountry ? await _localizationService.GetLocalizedAsync(orderCountry, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.ShippingCustomAttributes", _addressAttributeFormatter.FormatAttributesAsync(orderAddress?.CustomAttributes ?? string.Empty), true));

            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
            var paymentMethodName = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.PaymentMethodSystemName;
            tokens.Add(new Token("Order.PaymentMethod", paymentMethodName));
            tokens.Add(new Token("Order.VatNumber", order.VatNumber));
            var sbCustomValues = new StringBuilder();
            var customValues = _paymentService.DeserializeCustomValues(order);
            if (customValues != null)
            {
                foreach (var item in customValues)
                {
                    sbCustomValues.AppendFormat("{0}: {1}", WebUtility.HtmlEncode(item.Key), WebUtility.HtmlEncode(item.Value != null ? item.Value.ToString() : string.Empty));
                    sbCustomValues.Append("\\n");
                }
            }

            tokens.Add(new Token("Order.CustomValues", sbCustomValues.ToString(), true));

            var language = await _languageService.GetLanguageByIdAsync(languageId);
            if (language != null && !string.IsNullOrEmpty(language.LanguageCulture))
            {
                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                var createdOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, await _dateTimeHelper.GetCustomerTimeZoneAsync(customer));
                tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
            }
            else
            {
                tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
            }

            var orderUrl = await RouteUrlAsync(order.StoreId, "OrderDetails", new { orderId = order.Id });
            tokens.Add(new Token("Order.OrderURLForCustomer", orderUrl, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(order, tokens);
        }

        protected virtual async Task<string> RouteUrlAsync(int storeId = 0, string routeName = null, object routeValues = null)
        {
            //try to get a store by the passed identifier
            var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync()
                ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate the relative URL
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = urlHelper.RouteUrl(routeName, routeValues);

            //compose the result
            return new Uri(new Uri(store.Url), url).AbsoluteUri;
        }

        public virtual async Task AddShipmentTokensAsync(IList<Token> tokens, Shipment shipment, int languageId)
        {
            tokens.Add(new Token("Shipment.ShipmentNumber", shipment.Id));
            tokens.Add(new Token("Shipment.TrackingNumber", shipment.TrackingNumber));
            var trackingNumberUrl = string.Empty;
            if (!string.IsNullOrEmpty(shipment.TrackingNumber))
            {
                var shipmentService = NopInstance.Load<IShipmentService>();
                var shipmentTracker = await shipmentService.GetShipmentTrackerAsync(shipment);
                if (shipmentTracker != null)
                    trackingNumberUrl = await shipmentTracker.GetUrlAsync(shipment.TrackingNumber);
            }

            tokens.Add(new Token("Shipment.TrackingNumberURL", trackingNumberUrl, true));
            var shipmentUrl = await RouteUrlAsync((await _orderService.GetOrderByIdAsync(shipment.OrderId)).StoreId, "ShipmentDetails", new { shipmentId = shipment.Id });
            tokens.Add(new Token("Shipment.URLForCustomer", shipmentUrl, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(shipment, tokens);
        }

        public virtual async Task AddOrderRefundedTokensAsync(IList<Token> tokens, Order order, decimal refundedAmount)
        {
            //should we convert it to customer currency?
            //most probably, no. It can cause some rounding or legal issues
            //furthermore, exchange rate could be changed
            //so let's display it the primary store currency

            var primaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            var refundedAmountStr = await _priceFormatter.FormatPriceAsync(refundedAmount, true, primaryStoreCurrencyCode, false, (await _workContext.GetWorkingLanguageAsync()).Id);

            tokens.Add(new Token("Order.AmountRefunded", refundedAmountStr));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(order, tokens);
        }

        public virtual async Task AddOrderNoteTokensAsync(IList<Token> tokens, OrderNote orderNote)
        {
            var order = await _orderService.GetOrderByIdAsync(orderNote.OrderId);

            tokens.Add(new Token("Order.NewNoteText", _orderService.FormatOrderNoteText(orderNote), true));
            var orderNoteAttachmentUrl = await RouteUrlAsync(order.StoreId, "GetOrderNoteFile", new { ordernoteid = orderNote.Id });
            tokens.Add(new Token("Order.OrderNoteAttachmentUrl", orderNoteAttachmentUrl, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(orderNote, tokens);
        }

        public async Task AddRecurringPaymentTokensAsync(IList<Token> tokens, RecurringPayment recurringPayment)
        {
            tokens.Add(new Token("RecurringPayment.ID", recurringPayment.Id));
            tokens.Add(new Token("RecurringPayment.CancelAfterFailedPayment",
                recurringPayment.LastPaymentFailed && _paymentSettings.CancelRecurringPaymentsAfterFailedPayment));
            if (await _orderService.GetOrderByIdAsync(recurringPayment.InitialOrderId) is Order order)
                tokens.Add(new Token("RecurringPayment.RecurringPaymentType", (await _paymentService.GetRecurringPaymentTypeAsync(order.PaymentMethodSystemName)).ToString()));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(recurringPayment, tokens);
        }

        public async Task AddNewsLetterSubscriptionTokensAsync(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));

            var activationUrl = await RouteUrlAsync(routeName: "NewsletterActivation", routeValues: new { token = subscription.NewsLetterSubscriptionGuid, active = "true" });
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deactivationUrl = await RouteUrlAsync(routeName: "NewsletterActivation", routeValues: new { token = subscription.NewsLetterSubscriptionGuid, active = "false" });
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deactivationUrl, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(subscription, tokens);
        }

        public async Task AddProductTokensAsync(IList<Token> tokens, Product product, int languageId)
        {
            var productService = NopInstance.Load<IProductService>();
            tokens.Add(new Token("Product.ID", product.Id));
            tokens.Add(new Token("Product.Name", await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId)));
            tokens.Add(new Token("Product.ShortDescription", await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription, languageId), true));
            tokens.Add(new Token("Product.SKU", product.Sku));
            tokens.Add(new Token("Product.StockQuantity", await productService.GetTotalStockQuantityAsync(product)));

            var productUrl = await RouteUrlAsync(routeName: "Product", routeValues: new { SeName = await _urlRecordService.GetSeNameAsync(product) });

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(product, tokens);
        }

        public async Task AddReturnRequestTokensAsync(IList<Token> tokens, ReturnRequest returnRequest, OrderItem orderItem)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            tokens.Add(new Token("ReturnRequest.CustomNumber", returnRequest.CustomNumber));
            tokens.Add(new Token("ReturnRequest.OrderId", orderItem.OrderId));
            tokens.Add(new Token("ReturnRequest.Product.Quantity", returnRequest.Quantity));
            tokens.Add(new Token("ReturnRequest.Product.Name", product.Name));
            tokens.Add(new Token("ReturnRequest.Reason", returnRequest.ReasonForReturn));
            tokens.Add(new Token("ReturnRequest.RequestedAction", returnRequest.RequestedAction));
            tokens.Add(new Token("ReturnRequest.CustomerComment", _htmlFormatter.FormatText(returnRequest.CustomerComments, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.StaffNotes", _htmlFormatter.FormatText(returnRequest.StaffNotes, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.Status", await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus)));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(returnRequest, tokens);
        }

        public async Task AddForumTopicTokensAsync(IList<Token> tokens, ForumTopic forumTopic,
            int? friendlyForumTopicPageIndex = null, int? appendedPostIdentifierAnchor = null)
        {
            var forumService = NopInstance.Load<IForumService>();
            string topicUrl;
            if (friendlyForumTopicPageIndex.HasValue && friendlyForumTopicPageIndex.Value > 1)
                topicUrl = await RouteUrlAsync(routeName: "TopicSlugPaged", routeValues: new { id = forumTopic.Id, slug = await forumService.GetTopicSeNameAsync(forumTopic), pageNumber = friendlyForumTopicPageIndex.Value });
            else
                topicUrl = await RouteUrlAsync(routeName: "TopicSlug", routeValues: new { id = forumTopic.Id, slug = await forumService.GetTopicSeNameAsync(forumTopic) });
            if (appendedPostIdentifierAnchor.HasValue && appendedPostIdentifierAnchor.Value > 0)
                topicUrl = $"{topicUrl}#{appendedPostIdentifierAnchor.Value}";
            tokens.Add(new Token("Forums.TopicURL", topicUrl, true));
            tokens.Add(new Token("Forums.TopicName", forumTopic.Subject));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(forumTopic, tokens);
        }

        public async Task AddForumTokensAsync(IList<Token> tokens, Forum forum)
        {
            var forumService = NopInstance.Load<IForumService>();
            var forumUrl = await RouteUrlAsync(routeName: "ForumSlug", routeValues: new { id = forum.Id, slug = await forumService.GetForumSeNameAsync(forum) });
            tokens.Add(new Token("Forums.ForumURL", forumUrl, true));
            tokens.Add(new Token("Forums.ForumName", forum.Name));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(forum, tokens);
        }

        public async Task AddAttributeCombinationTokensAsync(IList<Token> tokens, ProductAttributeCombination combination, int languageId)
        {
            //attributes
            //we cannot inject IProductAttributeFormatter into constructor because it'll cause circular references.
            //that's why we resolve it here this way
            var productAttributeFormatter = EngineContext.Current.Resolve<IProductAttributeFormatter>();

            var product = await _productService.GetProductByIdAsync(combination.ProductId);
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            var attributes = await productAttributeFormatter.FormatAttributesAsync(product,
                combination.AttributesXml,
                currentCustomer,
                currentStore,
                renderPrices: false);

            tokens.Add(new Token("AttributeCombination.Formatted", attributes, true));
            tokens.Add(new Token("AttributeCombination.SKU", await _productService.FormatSkuAsync(await _productService.GetProductByIdAsync(combination.ProductId), combination.AttributesXml)));
            tokens.Add(new Token("AttributeCombination.StockQuantity", combination.StockQuantity));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(combination, tokens);
        }

        public async Task AddBackInStockTokensAsync(IList<Token> tokens, BackInStockSubscription subscription)
        {
            var product = await _productService.GetProductByIdAsync(subscription.ProductId);

            tokens.Add(new Token("BackInStockSubscription.ProductName", product.Name));
            var productUrl = await RouteUrlAsync(subscription.StoreId, "Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) });
            tokens.Add(new Token("BackInStockSubscription.ProductUrl", productUrl, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(subscription, tokens);
        }

        public async Task AddForumPostTokensAsync(IList<Token> tokens, ForumPost forumPost)
        {
            var customer = await _customerService.GetCustomerByIdAsync(forumPost.CustomerId);

            var forumService = NopInstance.Load<IForumService>();
            tokens.Add(new Token("Forums.PostAuthor", await _customerService.FormatUsernameAsync(customer)));
            tokens.Add(new Token("Forums.PostBody", forumService.FormatPostText(forumPost), true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(forumPost, tokens);
        }

        public async Task AddPrivateMessageTokensAsync(IList<Token> tokens, PrivateMessage privateMessage)
        {
            var forumService = NopInstance.Load<IForumService>();
            tokens.Add(new Token("PrivateMessage.Subject", privateMessage.Subject));
            tokens.Add(new Token("PrivateMessage.Text", forumService.FormatPrivateMessageText(privateMessage), true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(privateMessage, tokens);
        }

        public async Task AddVendorTokensAsync(IList<Token> tokens, Vendor vendor)
        {
            tokens.Add(new Token("Vendor.Name", vendor.Name));
            tokens.Add(new Token("Vendor.Email", vendor.Email));

            var vendorAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(vendor, NopVendorDefaults.VendorAttributes);
            tokens.Add(new Token("Vendor.VendorAttributes", await _vendorAttributeFormatter.FormatAttributesAsync(vendorAttributesXml), true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(vendor, tokens);
        }

        public async Task AddGiftCardTokensAsync(IList<Token> tokens, GiftCard giftCard, int languageId)
        {
            tokens.Add(new Token("GiftCard.SenderName", giftCard.SenderName));
            tokens.Add(new Token("GiftCard.SenderEmail", giftCard.SenderEmail));
            tokens.Add(new Token("GiftCard.RecipientName", giftCard.RecipientName));
            tokens.Add(new Token("GiftCard.RecipientEmail", giftCard.RecipientEmail));

            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            tokens.Add(new Token("GiftCard.Amount", await _priceFormatter.FormatPriceAsync(giftCard.Amount, true, primaryStoreCurrency.CurrencyCode, false, languageId)));
            tokens.Add(new Token("GiftCard.CouponCode", giftCard.GiftCardCouponCode));

            var giftCardMessage = !string.IsNullOrWhiteSpace(giftCard.Message) ?
                _htmlFormatter.FormatText(giftCard.Message, false, true, false, false, false, false) : string.Empty;

            tokens.Add(new Token("GiftCard.Message", giftCardMessage, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(giftCard, tokens);
        }

        public async Task AddProductReviewTokensAsync(IList<Token> tokens, ProductReview productReview)
        {
            var product = await _productService.GetProductByIdAsync(productReview.ProductId);

            tokens.Add(new Token("ProductReview.ProductName", product.Name));
            tokens.Add(new Token("ProductReview.Title", productReview.Title));
            tokens.Add(new Token("ProductReview.IsApproved", productReview.IsApproved));
            tokens.Add(new Token("ProductReview.ReviewText", productReview.ReviewText));
            tokens.Add(new Token("ProductReview.ReplyText", productReview.ReplyText));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(productReview, tokens);
        }
    }
}
