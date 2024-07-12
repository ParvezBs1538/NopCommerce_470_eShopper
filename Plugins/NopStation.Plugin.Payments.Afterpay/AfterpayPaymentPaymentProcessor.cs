using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.Afterpay.Components;
using NopStation.Plugin.Payments.Afterpay.Services;

namespace NopStation.Plugin.Payments.Afterpay
{
    public class AfterpayPaymentPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly WidgetSettings _widgetSettings;
        private readonly IAfterpayPaymentService _afterpayRequestService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly AfterpayPaymentSettings _afterpayPaymentSettings;

        #endregion

        #region Ctor

        public AfterpayPaymentPaymentProcessor(IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderService orderService,
            ISettingService settingService,
            IWebHelper webHelper,
            IScheduleTaskService scheduleTaskService,
            WidgetSettings widgetSettings,
            IAfterpayPaymentService afterpayRequestService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            AfterpayPaymentSettings afterpayPaymentSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderService = orderService;
            _settingService = settingService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;
            _widgetSettings = widgetSettings;
            _afterpayRequestService = afterpayRequestService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _afterpayPaymentSettings = afterpayPaymentSettings;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending });
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var paymentUrlRequest = await _afterpayRequestService.GeneratePaymentUrlRequestAsync(order);
            var paymentUrlResponse = await _afterpayRequestService.GetResponseAsync(paymentUrlRequest);

            if (paymentUrlResponse == null)
            {
                _httpContextAccessor.HttpContext.Response.Redirect($"{_webHelper.GetStoreLocation()}OrderDetails/{order.Id}");
                return;
            }

            order.AuthorizationTransactionId = paymentUrlResponse.Token;
            await _orderService.UpdateOrderAsync(order);
            _httpContextAccessor.HttpContext.Response.Redirect(paymentUrlResponse.RedirectCheckoutUrl);
        }

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            if (string.IsNullOrEmpty(_afterpayPaymentSettings?.MerchantId) || string.IsNullOrEmpty(_afterpayPaymentSettings?.MerchantKey))
                return true;

            var maximumAmount = _afterpayPaymentSettings.MaximumAmount;
            var minimumAmount = _afterpayPaymentSettings.MinimumAmount;

            var orderTotal = (await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart)).shoppingCartTotal;

            if (orderTotal.HasValue && (orderTotal.Value > maximumAmount || orderTotal.Value < minimumAmount))
            {
                return true;
            }

            return false;
        }

        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(0m);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var orderId = refundPaymentRequest.Order.Id;

            var refundResponse = await _afterpayRequestService.RefundPaymentAsync(refundPaymentRequest.Order.AuthorizationTransactionId, refundPaymentRequest.AmountToRefund, orderId);
            if (refundResponse != null)
            {
                return new RefundPaymentResult { NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded };
            }

            return new RefundPaymentResult { Errors = new[] { "Refund Unsuccessful" } };
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

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AfterpayPayment/Configure";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AfterpayPaymentDefaults.AfterpayInstallmentWidgetZone });
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new AfterpayPaymentSettings
            {
                MinimumAmount = 1,
                MaximumAmount = 2000
            });

            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(AfterpayPaymentDefaults.PAYMENT_STATUS_UPDATE_TASK_TYPE);
            if (scheduleTask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new Nop.Core.Domain.ScheduleTasks.ScheduleTask()
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
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await this.InstallPluginAsync(new AfterpayPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<AfterpayPaymentSettings>();

            //task
            var task = await _scheduleTaskService.GetTaskByTypeAsync(AfterpayPaymentDefaults.PAYMENT_STATUS_UPDATE_TASK_TYPE);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await this.UninstallPluginAsync(new AfterpayPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.Afterpay.PaymentMethodDescription");
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Afterpay.Menu.Afterpay"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(AfterpayPaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Afterpay.Menu.Configuration"),
                    Url = "~/Admin/AfterpayPayment/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Afterpay"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/afterpay-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=afterpay-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Menu.Afterpay", "Afterpay"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Configuration", "Afterpay settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Configuration.Saved", "Afterpay settings saved successfully"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Instructions", "This payment method works by Afterpay user interface to pay most securely."),

                new KeyValuePair<string, string>("NopStation.Afterpay.Fields.RedirectionTip", "You will be redirected to Afterpay website"),
                new KeyValuePair<string, string>("NopStation.Afterpay.PaymentMethodDescription", "Pay by Afterpay"),

                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.UseSandbox.Hint", "Check to enable testing environment (sandbox)."),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.Debug", "Debug"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.Debug.Hint", "Debug mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.MerchantId", "Merchant id"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.MerchantId.Hint", "Please use valid merchant Id"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.MerchantKey", "Merchant key"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.MerchantKey.Hint", "Please use valid merchant key"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.MinimumAmount", "Minimum amount"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.MinimumAmount.Hint", "Please use valid minimum amount"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.MaximumAmount", "Maximum amount"),
                new KeyValuePair<string, string>("Admin.NopStation.Afterpay.Fields.MaximumAmount.Hint", "Please use valid maximum amount"),

                new KeyValuePair<string, string>("NopStation.Afterpay.Message.Notpaid", "Your order payment is not completed yet")
            };

            return list;
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentAfterpayViewComponent);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(AfterpayInstallmentViewComponent);
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        public bool HideInWidgetList => false;

        #endregion
    }
}
