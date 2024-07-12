using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.StripeAlipay.Components;
using NopStation.Plugin.Payments.StripeAlipay.Services;
using Stripe;
using Stripe.Checkout;
using Order = Nop.Core.Domain.Orders.Order;

namespace NopStation.Plugin.Payments.StripeAlipay
{
    public class StripeAlipayPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly StripeAlipayPaymentSettings _alipayPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly ILogger _logger;
        private readonly StripeManager _stripeManager;

        #endregion

        #region Ctor

        public StripeAlipayPaymentProcessor(StripeAlipayPaymentSettings alipayPaymentSettings,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWorkContext workContext,
            IHttpContextAccessor httpContextAccessor,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            IOrderService orderService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ICustomerService customerService,
            IProductService productService,
            IPictureService pictureService,
            IPaymentPluginManager paymentPluginManager,
            ILogger logger,
            StripeManager stripeManager)
        {
            _alipayPaymentSettings = alipayPaymentSettings;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _workContext = workContext;
            _httpContextAccessor = httpContextAccessor;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _orderService = orderService;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _customerService = customerService;
            _productService = productService;
            _pictureService = pictureService;
            _paymentPluginManager = paymentPluginManager;
            _logger = logger;
            _stripeManager = stripeManager;
        }

        #endregion

        #region Utilities

        protected async Task PrepareShippingDetailsAsync(SessionCreateOptions options, Order order)
        {
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired && await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0) is Nop.Core.Domain.Common.Address shippingAddress)
            {
                options.PaymentIntentData.Shipping = new ChargeShippingOptions()
                {
                    Address = new AddressOptions()
                    {
                        City = shippingAddress.City,
                        Line1 = shippingAddress.Address1,
                        Line2 = shippingAddress.Address2,
                        PostalCode = shippingAddress.ZipPostalCode,
                        Country = (await _countryService.GetCountryByAddressAsync(shippingAddress))?.TwoLetterIsoCode,
                        State = (await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress))?.Name
                    },
                    Name = $"{shippingAddress.FirstName} {shippingAddress.LastName}",
                    Carrier = order.ShippingMethod,
                    Phone = shippingAddress.PhoneNumber,
                };
            }
        }

        protected async Task PrepareLineItemsAsync(SessionCreateOptions options, Order order)
        {
            long totalPrice = 0;

            foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();

                var unitPrice = _stripeManager.ConvertCurrencyToLong(orderItem.UnitPriceInclTax, order.CurrencyRate);
                totalPrice += unitPrice * orderItem.Quantity;

                string description = null;
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(orderItem.AttributeDescription);
                    description = HttpUtility.HtmlDecode(doc.DocumentNode.InnerText);
                }

                options.LineItems.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = unitPrice,
                        Currency = order.CustomerCurrencyCode,
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                            Description = description,
                            Metadata = new Dictionary<string, string>
                            {
                                [StripeDefaults.ProductId] = product.Id.ToString(),
                                [StripeDefaults.ProductSku] = product.Sku
                            },
                            Images = new List<string> { (await _pictureService.GetPictureUrlAsync(picture, 200)).Url }
                        }
                    },
                    Quantity = orderItem.Quantity
                });
            }

            if (order.OrderShippingInclTax > 0)
            {
                var unitPrice = _stripeManager.ConvertCurrencyToLong(order.OrderShippingInclTax, order.CurrencyRate);
                totalPrice += unitPrice;

                options.LineItems.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = unitPrice,
                        Currency = order.CustomerCurrencyCode,
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = "Shipping Charge",
                            Description = order.ShippingMethod,
                            Metadata = new Dictionary<string, string>
                            {
                                [StripeDefaults.ShippingMethodSystemName] = order.ShippingRateComputationMethodSystemName
                            }
                        }
                    },
                    Quantity = 1
                });
            }

            if (order.PaymentMethodAdditionalFeeInclTax > 0)
            {
                var pm = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);

                var unitPrice = _stripeManager.ConvertCurrencyToLong(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                totalPrice += unitPrice;

                options.LineItems.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = unitPrice,
                        Currency = order.CustomerCurrencyCode,
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = "Payment Fee",
                            Description = pm?.PluginDescriptor?.FriendlyName ?? order.PaymentMethodSystemName,
                            Metadata = new Dictionary<string, string>
                            {
                                [StripeDefaults.PaymentMethodSystemName] = order.PaymentMethodSystemName
                            }
                        }
                    },
                    Quantity = 1
                });
            }

            var orderTotal = _stripeManager.ConvertCurrencyToLong(order.OrderTotal, order.CurrencyRate);
            if (orderTotal > totalPrice)
            {
                options.LineItems.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = orderTotal - totalPrice,
                        Currency = order.CustomerCurrencyCode,
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = "Others"
                        }
                    },
                    Quantity = 1
                });
            }
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            try
            {
                var order = postProcessPaymentRequest.Order;
                var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

                var alipayPaymentSettings = await _settingService.LoadSettingAsync<StripeAlipayPaymentSettings>(order.StoreId);
                StripeConfiguration.ApiKey = alipayPaymentSettings.ApiKey;

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                        "alipay"
                    },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = new Uri(_webHelper.GetStoreLocation()).Concat(urlHelper.RouteUrl("StripeAlipayCallback", new { orderId = order.Id })).AbsoluteUri,
                    PaymentIntentData = new SessionPaymentIntentDataOptions()
                    {
                        Metadata = new Dictionary<string, string> { [StripeDefaults.OrderId] = order.Id.ToString() },
                        ReceiptEmail = address?.Email
                    },
                    ClientReferenceId = customer?.CustomerGuid.ToString(),
                    CustomerEmail = customer?.Email ?? address.Email,
                    CancelUrl = new Uri(_webHelper.GetStoreLocation()).Concat(urlHelper.RouteUrl("StripeAlipayCallback", new { orderId = order.Id })).AbsoluteUri,
                };

                await PrepareLineItemsAsync(options, order);
                await PrepareShippingDetailsAsync(options, order);

                var service = new SessionService();
                var session = service.Create(options);

                order.AuthorizationTransactionId = session.PaymentIntentId;
                await _orderService.UpdateOrderAsync(order);

                _httpContextAccessor.HttpContext.Response.Redirect(session.Url);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
            }
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _alipayPaymentSettings.AdditionalFee, _alipayPaymentSettings.AdditionalFeePercentage);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            try
            {
                var order = refundPaymentRequest.Order;

                var alipayPaymentSettings = await _settingService.LoadSettingAsync<StripeAlipayPaymentSettings>(order.StoreId);
                StripeConfiguration.ApiKey = alipayPaymentSettings.ApiKey;

                var options = new RefundCreateOptions
                {
                    PaymentIntent = order.AuthorizationTransactionId,
                    Amount = _stripeManager.ConvertCurrencyToLong(refundPaymentRequest.AmountToRefund, order.CurrencyRate)
                };
                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                if (refund.Status == "succeeded" || refund.Status == "pending")
                    result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
                else
                    result.AddError(refund.FailureReason);
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }

            return result;
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(true);
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var result = new List<string>();

            if (_alipayPaymentSettings.SupportedCurrencyCodes.Any())
            {
                var currency = await _workContext.GetWorkingCurrencyAsync();
                if (!_alipayPaymentSettings.SupportedCurrencyCodes.Any(c => c.Equals(currency.CurrencyCode, StringComparison.InvariantCultureIgnoreCase)))
                    result.Add(await _localizationService.GetResourceAsync("NopStation.StripeAlipay.InvalidCurrency"));
            }

            return result;
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var request = new ProcessPaymentRequest();
            return Task.FromResult(request);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentStripeAlipay/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(StripeAlipayViewComponent);
        }

        public override async Task InstallAsync()
        {
            var alipayPaymentSettings = new StripeAlipayPaymentSettings
            {
                DescriptionText = "<p>You will redirected to Alipay to complete the payment.</p>",
                SupportedCurrencyCodes = new List<string> { "CNY", "AUD", "CAD", "EUR", "GBP", "HKD", "JPY", "MYR", "NZD", "SGD", "USD" }
            };
            await _settingService.SaveSettingAsync(alipayPaymentSettings);

            await this.InstallPluginAsync(new StripeAlipayPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<StripeAlipayPaymentSettings>();
            await this.UninstallPluginAsync(new StripeAlipayPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.StripeAlipay.PaymentMethodDescription");
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Menu.StripeAlipay", "Stripe Alipay"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Menu.Configuration", "Configuration"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration", "Stripe Alipay settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.AdditionalFee", "Additional fee"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.AdditionalFee.Hint", "The additional fee."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.DescriptionText", "Description"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.DescriptionText.Hint", "Enter info that will be shown to customers during checkout"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.ApiKey", "Api/Secret key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.ApiKey.Hint", "Enter Stripe api key (secret key)."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.PublishableKey", "Publishable key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.PublishableKey.Hint", "Enter Stripe publishable key."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.EnableWebhook", "Enable webhook"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.EnableWebhook.Hint", "Check to enable webhook. In that case, you need to configure webhook in stripe account."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.WebhookSecret", "Webhook secret"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.WebhookSecret.Hint", "Enter Stripe webhook secret."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.SupportedCurrencyCodes", "Supported currencies"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeAlipay.Configuration.Fields.SupportedCurrencyCodes.Hint", "Select all supported currencies. Keep it empty if all active currencies are supported."));

            list.Add(new KeyValuePair<string, string>("NopStation.StripeAlipay.PaymentMethodDescription", "Pay by Alipay"));
            list.Add(new KeyValuePair<string, string>("NopStation.StripeAlipay.InvalidCurrency", "Currency Not Supported"));

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.StripeAlipay.Menu.StripeAlipay"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(StripeAlipayPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.StripeAlipay.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/PaymentStripeAlipay/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "StripeAlipay.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/stripe-alipay-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=stripe-alipay-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => true;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        #endregion
    }
}