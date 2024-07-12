using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.Flutterwave.Components;
using NopStation.Plugin.Payments.Flutterwave.Services;

namespace NopStation.Plugin.Payments.Flutterwave
{
    public class FlutterwavePaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly FlutterwavePaymentSettings _flutterwavePaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly FlutterwaveManager _flutterwaveManager;
        private readonly ISettingService _settingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public FlutterwavePaymentProcessor(IWebHelper webHelper,
            FlutterwavePaymentSettings flutterwavePaymentSettings,
            ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            FlutterwaveManager flutterwaveManager,
            ISettingService settingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWorkContext workContext)
        {
            _webHelper = webHelper;
            _flutterwavePaymentSettings = flutterwavePaymentSettings;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _flutterwaveManager = flutterwaveManager;
            _settingService = settingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _workContext = workContext;
        }

        #endregion

        #region Properies

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentFlutterwave/Configure";
        }

        public override async Task InstallAsync()
        {
            //settings
            var flutterwavePaymentSettings = new FlutterwavePaymentSettings
            {
                Description = "<p>You will be redirected to Flutterwave gateway.</p>",
                UseSandbox = true
            };
            await _settingService.SaveSettingAsync(flutterwavePaymentSettings);

            await this.InstallPluginAsync(new FlutterwavePaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new FlutterwavePaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Flutterwave.RecurringNotSupported"));
            return result;
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(order.AuthorizationTransactionResult != "authorized");
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Flutterwave.CaptureNotSupported"));
            return result;
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _flutterwavePaymentSettings.AdditionalFee, _flutterwavePaymentSettings.AdditionalFeePercentage);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            await _flutterwaveManager.RemotePost(postProcessPaymentRequest.Order);
        }

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending });
        }

        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Flutterwave.RecurringNotSupported"));
            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return await _flutterwaveManager.RefundAsync(refundPaymentRequest);
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Flutterwave.VoidNotSupported"));
            return result;
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var result = new List<string>();
            return Task.FromResult<IList<string>>(result);
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public Type GetPublicViewComponent()
        {
            return typeof(FlutterwaveViewComponent);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Flutterwave.Menu.Flutterwave"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(FlutterwavePaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Flutterwave.Menu.Configuration"),
                    Url = "~/Admin/PaymentFlutterwave/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Flutterwave.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/flutterwave-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=flutterwave-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            if (menuItem.ChildNodes.Any())
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration", "Flutterwave payment settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Menu.Flutterwave", "Flutterwave"),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("NopStation.Flutterwave.CaptureNotSupported", "Capture method not supported"),
                new KeyValuePair<string, string>("NopStation.Flutterwave.RefundNotSupported", "Refund method not supported"),
                new KeyValuePair<string, string>("NopStation.Flutterwave.VoidNotSupported", "Void method not supported"),
                new KeyValuePair<string, string>("NopStation.Flutterwave.RecurringNotSupported", "Recurring method not supported"),

                new KeyValuePair<string, string>("NopStation.Flutterwave.Description", "Pay by Flutterwave"),

                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.SecretKey", "Secret key"),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.PublicKey", "Public key"),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.EncryptionKey", "Encryption key"),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.Description", "Description"),

                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),

                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.SecretKey.Hint", "The 'Secret key' for Flutterwave payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.PublicKey.Hint", "The 'Public key' for Flutterwave payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.EncryptionKey.Hint", "The 'Encryption key' for Flutterwave payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.Description.Hint", "Enter info that will be shown to customers during checkout."),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.Flutterwave.Configuration.Fields.UseSandbox.Hint", "Check to enable testing environment (sandbox)."),
            };

            return list;
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.Flutterwave.Description");
        }

        #endregion
    }
}
