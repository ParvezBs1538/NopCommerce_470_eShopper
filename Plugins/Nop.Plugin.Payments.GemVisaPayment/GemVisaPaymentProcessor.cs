using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.GemVisaPayment.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.GemVisaPayment
{
    /// <summary>
    /// GemVisaPayment payment processor
    /// </summary>
    public class GemVisaPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
    {

        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;

        private readonly ISettingService _settingService;
        private readonly IGemVisaPaymentService _gemVisaPaymentService;
        private readonly IWebHelper _webHelper;
        private readonly GemVisaPaymentSettings _gemVisaPaymentSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public GemVisaPaymentProcessor(IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            ISettingService settingService,
            IGemVisaPaymentService gemVisaPaymentService,
            IWebHelper webHelper,
            GemVisaPaymentSettings gemVisaPaymentSettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext)
        {

            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _settingService = settingService;
            _gemVisaPaymentService = gemVisaPaymentService;
            _webHelper = webHelper;
            _gemVisaPaymentSettings = gemVisaPaymentSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentGemVisa/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentGemVisa";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new GemVisaPaymentSettings
            {
            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.RedirectionTip", "You will be redirected to Latitude Financial Services");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.PaymentMethodDescription", "You will be redirected to Latitude Financial Services site to complete the payment");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.UseSandbox", "Use Sandbox");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.DebugMode", "Debug Mode");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.MerchantId", "Merchant Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.SecretKey", "Secret Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.InstallmentMonth", "Interest Free Month");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.MinimumAmount", "Minimum Amount");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.MinimumAmount.Hint", "Minimum amount that can be paid by Gem Visa");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.MaximumAmount", "Maximum Amount");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.MaximumAmount.Hint", "Maximum amount that can be paid by Gem Visa");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.InstallmentMonth.Hint", "This info will show on the public site. eg: Interest Free over 12 months");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.ShowInfoInProductDetailsPage", "Show On Product Details");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.GemVisa.Fields.ShowInfoInProductDetailsPage.Hint", "Gem Pay will show information on product details page, eg: Interest Free over 12 months");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<GemVisaPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Payments.GemVisa");


            base.Uninstall();
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var purchaseRequestBody = _gemVisaPaymentService.GenerateRequestBody(order);
            var purchaseResponse = _gemVisaPaymentService.GetResponse(purchaseRequestBody);
            if (purchaseResponse == null)
            {
                _httpContextAccessor.HttpContext.Response.Redirect(string.Format(GemVisaPaymentDefaults.ORDER_DETAILS_URL, _webHelper.GetStoreLocation(), order.Id));
                return;
            }
            var url = purchaseResponse.RedirectUrl;
            _httpContextAccessor.HttpContext.Response.Redirect(url);
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer);
            var maximumAmount = _gemVisaPaymentSettings.MaximumAmount;
            var minimumAmount = _gemVisaPaymentSettings.MinimumAmount;
            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart, out var _, out var _, out var _, out var _, out var _);
            if (orderTotal == null)
                return true;
            if (orderTotal.Value > maximumAmount || orderTotal.Value < minimumAmount)
            {
                return true;
            }
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0;
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public bool CanRePostProcessPayment(Order order)
        {
            return true;
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                GemVisaPaymentDefaults.GemVisaInstallmentWidgetZone
            };
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));
            else if (widgetZone == GemVisaPaymentDefaults.GemVisaInstallmentWidgetZone)
            {
                return "GemVisaInstallmentMessage";
            }
            else
                return string.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.GemVisa.PaymentMethodDescription");

        public bool HideInWidgetList => false;

        #endregion
    }
}
