using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Payments.AfterpayPayment.Services;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Seo;
using Nop.Services.Tasks;
using Nop.Services.Tax;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Payments.AfterpayPayment
{
    /// <summary>
    /// QCardPayment payment processor
    /// </summary>
    public class AfterpayPaymentPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields

        private readonly IUrlRecordService _urlRecordService;
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressService _addressService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWorkContext _workContext;
        private readonly IDiscountService _discountService;
        private readonly WidgetSettings _widgetSettings;
        private readonly IAfterpayPaymentService _afterpayRequestService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderProcessingService _orderProcessingService;



        #endregion

        #region Ctor

        public AfterpayPaymentPaymentProcessor(
            IUrlRecordService urlRecordService,
            CurrencySettings currencySettings,
            IAddressService addressService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPaymentService paymentService,
            IProductService productService,
            ISettingService settingService,
            IStateProvinceService stateProvinceService,
            ITaxService taxService,
            IWebHelper webHelper,
            ILogger logger,
            IStoreContext storeContext,
            IScheduleTaskService scheduleTaskService,
            IWorkContext workContext,
            IDiscountService discountService,
            WidgetSettings widgetSettings,
            IAfterpayPaymentService afterpayRequestService,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IOrderProcessingService orderProcessingService)
        {
            _urlRecordService = urlRecordService;
            _currencySettings = currencySettings;
            _addressService = addressService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderService = orderService;
            _paymentService = paymentService;
            _productService = productService;
            _settingService = settingService;
            _stateProvinceService = stateProvinceService;
            _taxService = taxService;
            _webHelper = webHelper;
            _logger = logger;
            _storeContext = storeContext;
            _scheduleTaskService = scheduleTaskService;
            _workContext = workContext;
            _discountService = discountService;
            _widgetSettings = widgetSettings;
            _afterpayRequestService = afterpayRequestService;
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _orderProcessingService = orderProcessingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AfterpayPayment/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        /// 

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            throw new NotImplementedException();
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            throw new NotImplementedException();
        }
        public string GetPublicViewComponentName()
        {
            return "PaymentAfterpay";
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                PublicWidgetZones.OrderDetailsPageTop, AfterpayPaymentDefaults.AfterpayInstallmentWidgetZone
            };
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));
            if (widgetZone == PublicWidgetZones.OrderDetailsPageTop)
            {
                return "AfterpayInfo";
            }
            else if (widgetZone == AfterpayPaymentDefaults.AfterpayInstallmentWidgetZone)
            {
                return "AfterpayInstallment";
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override async Task InstallAsync()
        {

            _settingService.SaveSetting(new AfterpayPaymentSettings
            {
                UseSandbox = true,
                MerchantId = "41842",
                MerchantKey = "8cf3a3f7caff7329ac346631641da339e7d05ee1a4c3597c33fa95f2313c1c1f26364063e54f47dc05673f0ee293cb596d8af423a2e807950930daf3af81cd40",
                MinimumAmount = 1,
                MaximumAmount = 2000
            });

            var scheduleTask = _scheduleTaskService.GetTaskByType(AfterpayPaymentDefaults.PAYMENT_STATUS_UPDATE_TASK_TYPE);
            if (scheduleTask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Afterpay Payment Status Update",
                    Seconds = 15 * 60,
                    Type = AfterpayPaymentDefaults.PAYMENT_STATUS_UPDATE_TASK_TYPE,
                    StopOnError = false
                });
            }

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME);
                _settingService.SaveSetting(_widgetSettings);
            }

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.Fields.RedirectionTip", "You will be redirected to Afterpay website");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.PaymentMethodDescription", "You will be redirected to Afterpay site to complete the payment");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.ConfigurationModel.Fields.MerchantId.Required", "Merchant id is required");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.ConfigurationModel.Fields.MerchantKey.Required", "Merchant Key is required");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.Fields.UseSandbox", "Use Sandbox");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.Fields.Debug", "Debug");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.Fields.MerchantId", "Merchant Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.Fields.MerchantKey", "Merchant Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.Fields.MinimumAmount", "Minimum Amount");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.Fields.MaximumAmount", "Maximum Amount");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Afterpay.Message.Notpaid", "Your order payment is not completed yet");

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<AfterpayPaymentSettings>();

            //task
            var task = await _scheduleTaskService.GetTaskByTypeAsync(AfterpayPaymentDefaults.PAYMENT_STATUS_UPDATE_TASK_TYPE);
            if (task != null)
                await _scheduleTaskService.DeleteTask(task);

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME);
                _settingService.SaveSetting(_widgetSettings);
            }
            //locals
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.Fields.RedirectionTip");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.PaymentMethodDescription");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.ConfigurationModel.Fields.MerchantId.Required");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.ConfigurationModel.Fields.MerchantKey.Required");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.Fields.UseSandbox");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.Fields.MerchantId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.Fields.MerchantKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.Fields.Debug");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.Fields.MinimumAmount");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.Fields.MaximumAmount");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Afterpay.Message.Notpaid");

            base.Uninstall();
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var paymentUrlRequest = _afterpayRequestService.GeneratePaymentUrlRequest(order);
            var paymentUrlResponse = _afterpayRequestService.GetResponse(paymentUrlRequest);
            order.AuthorizationTransactionId = paymentUrlResponse.Token;
            _orderService.UpdateOrder(order);
            if(paymentUrlResponse==null)
            {
                _httpContextAccessor.HttpContext.Response.Redirect(_webHelper.GetStoreLocation());
            }
            var url = paymentUrlResponse.RedirectCheckoutUrl;
            _httpContextAccessor.HttpContext.Response.Redirect(url);
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            var storeScope = _storeContext.CurrentStore.Id;
            var afterpayPaymentSettings = _settingService.LoadSetting<AfterpayPaymentSettings>(storeScope);
            var maximumAmount = afterpayPaymentSettings.MaximumAmount;
            var minimumAmount = afterpayPaymentSettings.MinimumAmount;
            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart, out var orderTotalDiscountAmountBase, out var _, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount);
            if (orderTotal.HasValue && (orderTotal.Value > maximumAmount || orderTotal.Value < minimumAmount))
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
            var order = refundPaymentRequest.Order;
            bool canRefunableAmount = _orderProcessingService.CanRefund(order);
            //refund previously captured payment
            var amount = refundPaymentRequest.AmountToRefund <= refundPaymentRequest.Order.OrderTotal
                ? (decimal?)refundPaymentRequest.AmountToRefund
                : null;
            if (refundPaymentRequest.IsPartialRefund && refundPaymentRequest.AmountToRefund == refundPaymentRequest.Order.OrderTotal)
            {
                amount = null;
            }
            var orderId = order.Id;
            var token = refundPaymentRequest.Order.AuthorizationTransactionId;
            //var amount = refundPaymentRequest.AmountToRefund;
            if (!amount.HasValue)
            {
                return new RefundPaymentResult { Errors = new[] { "Partial refund amount can not be greater or equal to order total" } };
            }
            if (!canRefunableAmount)
            {
                return new RefundPaymentResult { Errors = new[] { "Already refunded!" } };
            }
            var refundResponse = _afterpayRequestService.RefundPayment(token, amount.Value, orderId);
            if (refundResponse != null)
            {
                if (!refundPaymentRequest.IsPartialRefund)
                {
                    order.OrderStatus = OrderStatus.Cancelled;
                    _orderService.UpdateOrder(order);
                }
                return new RefundPaymentResult { NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded };
            }
            return new RefundPaymentResult { Errors = new[] { "Refund Unsuccessful" } };
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

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => true;

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
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.Afterpay.PaymentMethodDescription");
        }

        public bool HideInWidgetList => false;

        #endregion
    }
}
