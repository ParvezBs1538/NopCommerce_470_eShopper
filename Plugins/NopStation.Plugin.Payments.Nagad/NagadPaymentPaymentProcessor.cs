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
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.Nagad.Components;
using NopStation.Plugin.Payments.Nagad.Services;

namespace NopStation.Plugin.Payments.Nagad
{
    public class NagadPaymentPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly WidgetSettings _widgetSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPermissionService _permissionService;
        private readonly INagadPaymentService _nagadPaymentService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly NagadPaymentSettings _nagadPaymentSettings;

        #endregion

        #region Ctor

        public NagadPaymentPaymentProcessor(IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderService orderService,
            ISettingService settingService,
            IWebHelper webHelper,
            WidgetSettings widgetSettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPermissionService permissionService,
            INagadPaymentService nagadPaymentService,
            INopStationCoreService nopStationCoreService,
            NagadPaymentSettings nagadPaymentSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderService = orderService;
            _settingService = settingService;
            _webHelper = webHelper;
            _widgetSettings = widgetSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _permissionService = permissionService;
            _nagadPaymentService = nagadPaymentService;
            _nopStationCoreService = nopStationCoreService;
            _nagadPaymentSettings = nagadPaymentSettings;
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

            var nagadPaymentInit = await _nagadPaymentService.NagadPaymentInitializationAsync(order);
            if (nagadPaymentInit != null)
            {
                var nagadRedirectUrl = await _nagadPaymentService.NagadPaymentOrderCompleteAsync(order, nagadPaymentInit);
                if (!string.IsNullOrEmpty(nagadRedirectUrl))
                {
                    order.AuthorizationTransactionId = nagadPaymentInit.PaymentReferenceId;
                    order.AuthorizationTransactionCode = nagadPaymentInit.Challenge;
                    await _orderService.UpdateOrderAsync(order);

                    _httpContextAccessor.HttpContext.Response.Redirect(nagadRedirectUrl);
                    return;
                }
            }
            _httpContextAccessor.HttpContext.Response.Redirect($"{_webHelper.GetStoreLocation()}OrderDetails/{order.Id}");
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            if (string.IsNullOrEmpty(_nagadPaymentSettings?.MerchantId) || string.IsNullOrEmpty(_nagadPaymentSettings?.NPGPublicKey)
                || string.IsNullOrEmpty(_nagadPaymentSettings?.MSPrivateKey))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart, _nagadPaymentSettings.AdditionalFee,
                _nagadPaymentSettings.AdditionalFeePercentage);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
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
            return $"{_webHelper.GetStoreLocation()}Admin/NagadPayment/Configure";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { });
        }

        public override async Task InstallAsync()
        {
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(NagadPaymentDefaults.PLUGIN_SYSTEM_NAME))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(NagadPaymentDefaults.PLUGIN_SYSTEM_NAME);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await this.InstallPluginAsync(new NagadPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<NagadPaymentSettings>();

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(NagadPaymentDefaults.PLUGIN_SYSTEM_NAME))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(NagadPaymentDefaults.PLUGIN_SYSTEM_NAME);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await this.UninstallPluginAsync(new NagadPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.Nagad.PaymentMethodDescription");
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Nagad.Menu.Nagad"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(NagadPaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Nagad.Menu.Configuration"),
                    Url = "~/Admin/NagadPayment/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Nagad"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/nagad-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=nagad-payment",
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
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Menu.Nagad", "Nagad"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration", "Nagad settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Saved", "Nagad settings saved successfully"),

                new KeyValuePair<string, string>("NopStation.Nagad.Fields.RedirectionTip", "You will be redirected to Nagad website"),
                new KeyValuePair<string, string>("NopStation.Nagad.PaymentMethodDescription", "Pay by Nagad"),

                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.Description", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.Description.Hint", "Payment description, that will be shown to customer"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.UseSandbox.Hint", "Check to enable testing environment (sandbox)."),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.MerchantId", "Merchant id"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.MerchantId.Hint", "Please use valid merchant Id"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.NPGPublicKey", "NPG Public Key"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.NPGPublicKey.Hint", "Please use valid NPG Public Key, provided by Nagad"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.MSPrivateKey", "MS Private Key"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.MSPrivateKey.Hint", "Please use valid MS Private Key, provided by Nagad"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.AdditionalFee.Hint", "The additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.Nagad.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),

                new KeyValuePair<string, string>("NopStation.Nagad.Message.Notpaid", "Your order payment is not completed yet")
            };

            return list;
        }

        public Type GetPublicViewComponent()
        {
            return typeof(NagadPaymentViewComponent);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return null;
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        public bool HideInWidgetList => false;

        #endregion
    }
}
