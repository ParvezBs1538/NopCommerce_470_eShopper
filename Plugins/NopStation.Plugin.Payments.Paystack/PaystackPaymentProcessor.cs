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
using NopStation.Plugin.Payments.Paystack.Components;
using NopStation.Plugin.Payments.Paystack.Services;

namespace NopStation.Plugin.Payments.Paystack
{
    public class PaystackPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly PaystackPaymentSettings _paystackPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly PaystackManager _paystackManager;
        private readonly ISettingService _settingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PaystackPaymentProcessor(IWebHelper webHelper,
            PaystackPaymentSettings paystackPaymentSettings,
            ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            PaystackManager paystackManager,
            ISettingService settingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWorkContext workContext)
        {
            _webHelper = webHelper;
            _paystackPaymentSettings = paystackPaymentSettings;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _paystackManager = paystackManager;
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPaystack/Configure";
        }

        public override async Task InstallAsync()
        {
            //settings
            var paystackPaymentSettings = new PaystackPaymentSettings
            {
                Description = "<p>You will be redirected to Paystack gateway.</p>",
                Currencies = new List<string>() { "NGN" }
            };
            await _settingService.SaveSettingAsync(paystackPaymentSettings);

            await this.InstallPluginAsync(new PaystackPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new PaystackPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Paystack.RecurringNotSupported"));
            return result;
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.PaymentStatus != PaymentStatus.Pending)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Paystack.CaptureNotSupported"));
            return result;
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _paystackPaymentSettings.AdditionalFee, _paystackPaymentSettings.AdditionalFeePercentage);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var url = await _paystackManager.GetPaymentLinkAsync(order);

            if (!string.IsNullOrWhiteSpace(url))
                _httpContextAccessor.HttpContext.Response.Redirect(url);
        }

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending });
        }

        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Paystack.RecurringNotSupported"));
            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return await _paystackManager.RefundAsync(refundPaymentRequest);
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Paystack.VoidNotSupported"));
            return result;
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var result = new List<string>();

            if (_paystackPaymentSettings.Currencies.Any())
            {
                var currency = await _workContext.GetWorkingCurrencyAsync();
                if (!_paystackPaymentSettings.Currencies.Any(c => c.Equals(currency.CurrencyCode, StringComparison.InvariantCultureIgnoreCase)))
                    result.Add(await _localizationService.GetResourceAsync("NopStation.Paystack.InvalidCurrency"));
            }

            return result;
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaystackViewComponent);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Paystack.Menu.Paystack"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(PaystackPaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Paystack.Menu.Configuration"),
                    Url = "~/Admin/PaymentPaystack/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Paystack.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/paystack-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=paystack-payment",
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
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration", "Paystack payment settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Menu.Paystack", "Paystack"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.Paystack.CaptureNotSupported", "Capture method not supported"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.RefundNotSupported", "Refund method not supported"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.VoidNotSupported", "Void method not supported"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.RecurringNotSupported", "Recurring method not supported"),

                new KeyValuePair<string, string>("NopStation.Paystack.Description", "Pay by Paystack"),
                new KeyValuePair<string, string>("NopStation.Paystack.InvalidCurrency", "Currency Not Supported"),

                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.PublicKey", "Public key"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.SecretKey", "Secret key"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.Description", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.Channels", "Channels"),

                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),

                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.Currencies", "Currencies"),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.Currencies.Hint", "Select supported currencies."),

                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.PublicKey.Hint", "The 'Public key' for Paystack payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.SecretKey.Hint", "The 'Secret key' for Paystack payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.Description.Hint", "Enter info that will be shown to customers during checkout."),
                new KeyValuePair<string, string>("Admin.NopStation.Paystack.Configuration.Fields.Channels.Hint", "Select available payment channels.")
            };

            return list;
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.Paystack.Description");
        }

        #endregion
    }
}
