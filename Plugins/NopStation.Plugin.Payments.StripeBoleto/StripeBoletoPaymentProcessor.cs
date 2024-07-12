using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.StripeBoleto.Components;
using NopStation.Plugin.Payments.StripeBoleto.Models;

namespace NopStation.Plugin.Payments.StripeBoleto
{
    public class StripeBoletoPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly StripeBoletoPaymentSettings _boletoPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public StripeBoletoPaymentProcessor(StripeBoletoPaymentSettings boletoPaymentSettings,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWorkContext workContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _boletoPaymentSettings = boletoPaymentSettings;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _workContext = workContext;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var url = $"{_webHelper.GetStoreLocation()}stripeboleto/payment/{postProcessPaymentRequest.Order.Id}";
            _httpContextAccessor.HttpContext.Response.Redirect(url);

            return Task.CompletedTask;
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _boletoPaymentSettings.AdditionalFee, _boletoPaymentSettings.AdditionalFeePercentage);
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

            return Task.FromResult(string.IsNullOrWhiteSpace(order.AuthorizationTransactionId));
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var result = new List<string>();

            if (_boletoPaymentSettings.SupportedCurrencyCodes.Any())
            {
                var currency = await _workContext.GetWorkingCurrencyAsync();
                if (!_boletoPaymentSettings.SupportedCurrencyCodes.Any(c => c.Equals(currency.CurrencyCode, StringComparison.InvariantCultureIgnoreCase)))
                    result.Add(await _localizationService.GetResourceAsync("NopStation.StripeBoleto.InvalidCurrency"));
            }


            if (!form.ContainsKey(nameof(PaymentInfoModel.TaxID)))
                result.Add(await _localizationService.GetResourceAsync("NopStation.StripeBoleto.TaxID.Required"));

            return result;
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var request = new ProcessPaymentRequest();
            if (form.TryGetValue(nameof(PaymentInfoModel.TaxID), out var val))
                request.CustomValues.Add(BoletoDefaults.TaxID, val.ToString());

            return Task.FromResult(request);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentStripeBoleto/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(StripeBoletoViewComponent);
        }

        public override async Task InstallAsync()
        {
            var boletoPaymentSettings = new StripeBoletoPaymentSettings()
            { 
                SupportedCurrencyCodes = new List<string> { "BRL" }
            };

            await _settingService.SaveSettingAsync(boletoPaymentSettings);

            await this.InstallPluginAsync(new StripeBoletoPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<StripeBoletoPaymentSettings>();
            await this.UninstallPluginAsync(new StripeBoletoPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.StripeBoleto.PaymentMethodDescription");
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Menu.StripeBoleto", "Stripe Boleto"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Menu.Configuration", "Configuration"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration", "Stripe Boleto settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.AdditionalFee", "Additional fee"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.AdditionalFee.Hint", "The additional fee."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.DescriptionText", "Description"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.DescriptionText.Hint", "Enter info that will be shown to customers during checkout"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.ApiKey", "Api/Secret key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.ApiKey.Hint", "Enter Stripe api key (secret key)."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.PublishableKey", "Publishable key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.PublishableKey.Hint", "Enter Stripe publishable key."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.WebhookSecret", "Webhook secret"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.WebhookSecret.Hint", "Enter Stripe webhook secret."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.SendOrderInfoToStripe", "Send order info to Stripe"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.SendOrderInfoToStripe.Hint", "Check to send order information to Stripe."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.SupportedCurrencyCodes", "Supported currencies"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeBoleto.Configuration.Fields.SupportedCurrencyCodes.Hint", "Select all supported currencies. Keep it empty if all active currencies are supported."));

            list.Add(new KeyValuePair<string, string>("NopStation.StripeBoleto.PaymentMethodDescription", "Pay by Stripe Boleto"));
            list.Add(new KeyValuePair<string, string>("NopStation.StripeBoleto.TaxID", "Tax ID"));
            list.Add(new KeyValuePair<string, string>("NopStation.StripeBoleto.TaxID.Required", "Tax ID is required."));
            list.Add(new KeyValuePair<string, string>("NopStation.StripeBoleto.InvalidCurrency", "Currency Not Supported"));

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.StripeBoleto.Menu.StripeBoleto"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(StripeBoletoPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.StripeBoleto.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/PaymentStripeBoleto/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "StripeBoleto.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/stripe-Boleto-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=stripe-Boleto-payment",
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

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => true;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        #endregion
    }
}