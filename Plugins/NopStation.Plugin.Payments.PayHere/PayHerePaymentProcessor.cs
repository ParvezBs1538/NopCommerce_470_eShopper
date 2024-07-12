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
using NopStation.Plugin.Payments.PayHere.Components;
using NopStation.Plugin.Payments.PayHere.Services;

namespace NopStation.Plugin.Payments.PayHere
{
    public class PayHerePaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly PayHerePaymentSettings _payHerePaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly PayHereManager _payHereManager;
        private readonly ISettingService _settingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PayHerePaymentProcessor(IWebHelper webHelper,
            PayHerePaymentSettings payHerePaymentSettings,
            ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            PayHereManager payHereManager,
            ISettingService settingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWorkContext workContext)
        {
            _webHelper = webHelper;
            _payHerePaymentSettings = payHerePaymentSettings;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _payHereManager = payHereManager;
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPayHere/Configure";
        }

        public override async Task InstallAsync()
        {
            //settings
            var payHerePaymentSettings = new PayHerePaymentSettings
            {
                Description = "<p>You will be redirected to PayHere gateway.</p>",
                UseSandbox = true
            };
            await _settingService.SaveSettingAsync(payHerePaymentSettings);

            await this.InstallPluginAsync(new PayHerePaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new PayHerePaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.PayHere.RecurringNotSupported"));
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
            result.AddError(await _localizationService.GetResourceAsync("NopStation.PayHere.CaptureNotSupported"));
            return result;
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _payHerePaymentSettings.AdditionalFee, _payHerePaymentSettings.AdditionalFeePercentage);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            await _payHereManager.RemotePost(postProcessPaymentRequest.Order);
        }

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending });
        }

        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.PayHere.RecurringNotSupported"));
            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return await _payHereManager.RefundAsync(refundPaymentRequest);
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.PayHere.VoidNotSupported"));
            return result;
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var result = new List<string>();
            var currency = await _workContext.GetWorkingCurrencyAsync();
            var currencies = new List<string>() { "LKR", "USD", "GBP", "EUR", "AUD" };

            if (!currencies.Contains(currency.CurrencyCode.ToUpper()))
                result.Add(await _localizationService.GetResourceAsync("NopStation.PayHere.InvalidCurrency"));

            return result;
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PayHereViewComponent);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PayHere.Menu.PayHere"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(PayHerePaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PayHere.Menu.Configuration"),
                    Url = "~/Admin/PaymentPayHere/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "PayHere.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/payhere-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=payhere-payment",
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
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration", "PayHere payment settings"),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Menu.PayHere", "PayHere"),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("NopStation.PayHere.CaptureNotSupported", "Capture method not supported"),
                new KeyValuePair<string, string>("NopStation.PayHere.RefundNotSupported", "Refund method not supported"),
                new KeyValuePair<string, string>("NopStation.PayHere.VoidNotSupported", "Void method not supported"),
                new KeyValuePair<string, string>("NopStation.PayHere.RecurringNotSupported", "Recurring method not supported"),

                new KeyValuePair<string, string>("NopStation.PayHere.Description", "Pay by PayHere"),
                new KeyValuePair<string, string>("NopStation.PayHere.InvalidCurrency", "Currency Not Supported"),

                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.MerchantId", "Merchant ID"),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.MerchantSecret", "Merchant secret"),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.Description", "Description"),

                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),

                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.MerchantId.Hint", "The 'Merchant ID' for PayHere payment."),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.MerchantSecret.Hint", "The 'Merchant secret' for PayHere payment."),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.Description.Hint", "Enter info that will be shown to customers during checkout."),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.PayHere.Configuration.Fields.UseSandbox.Hint", "Check to enable testing environment (sandbox)."),
            };

            return list;
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.PayHere.Description");
        }

        #endregion
    }
}
