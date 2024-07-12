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
using NopStation.Plugin.Payments.Instamojo.Components;
using NopStation.Plugin.Payments.Instamojo.Services;

namespace NopStation.Plugin.Payments.Instamojo
{
    public class InstamojoPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly InstamojoPaymentSettings _instamojoPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly InstamojoManager _instamojoManager;
        private readonly ISettingService _settingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public InstamojoPaymentProcessor(IWebHelper webHelper,
            InstamojoPaymentSettings instamojoPaymentSettings,
            ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            InstamojoManager instamojoManager,
            ISettingService settingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWorkContext workContext)
        {
            _webHelper = webHelper;
            _instamojoPaymentSettings = instamojoPaymentSettings;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _instamojoManager = instamojoManager;
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentInstamojo/Configure";
        }

        public override async Task InstallAsync()
        {
            //settings
            var instamojoPaymentSettings = new InstamojoPaymentSettings
            {
                Description = "<p>You will be redirected to Instamojo gateway.</p>",
                UseSandbox = true,
                EnableSendEmail = false,
                EnableSendSMS = false
            };
            await _settingService.SaveSettingAsync(instamojoPaymentSettings);

            await this.InstallPluginAsync(new InstamojoPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new InstamojoPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Instamojo.RecurringNotSupported"));
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
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Instamojo.CaptureNotSupported"));
            return result;
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _instamojoPaymentSettings.AdditionalFee, _instamojoPaymentSettings.AdditionalFeePercentage);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var url = await _instamojoManager.GetPaymentUrlAsync(order);

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
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Instamojo.RecurringNotSupported"));
            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return await _instamojoManager.RefundAsync(refundPaymentRequest);
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Instamojo.VoidNotSupported"));
            return result;
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var result = new List<string>();
            var currency = await _workContext.GetWorkingCurrencyAsync();

            if (!currency.CurrencyCode.Equals("INR", StringComparison.InvariantCultureIgnoreCase))
                result.Add(await _localizationService.GetResourceAsync("NopStation.Instamojo.InvalidCurrency"));

            return result;
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public Type GetPublicViewComponent()
        {
            return typeof(InstamojoViewComponent);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Instamojo.Menu.Instamojo"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(InstamojoPaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Instamojo.Menu.Configuration"),
                    Url = "~/Admin/PaymentInstamojo/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Instamojo.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/instamojo-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=instamojo-payment",
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
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration", "Instamojo payment settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Menu.Instamojo", "Instamojo"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("NopStation.Instamojo.CaptureNotSupported", "Capture method not supported"),
                new KeyValuePair<string, string>("NopStation.Instamojo.RefundNotSupported", "Refund method not supported"),
                new KeyValuePair<string, string>("NopStation.Instamojo.VoidNotSupported", "Void method not supported"),
                new KeyValuePair<string, string>("NopStation.Instamojo.RecurringNotSupported", "Recurring method not supported"),

                new KeyValuePair<string, string>("NopStation.Instamojo.Description", "Pay by Instamojo"),
                new KeyValuePair<string, string>("NopStation.Instamojo.InvalidCurrency", "Currency Not Supported"),

                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.PrivateApiKey", "Private API key"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.PrivateAuthToken", "Private auth token"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.PrivateSalt", "Private salt"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.Description", "Description"),

                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),

                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.PrivateApiKey.Hint", "The 'Private API key' for Instamojo payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.PrivateAuthToken.Hint", "The 'Private auth token' for Instamojo payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.PrivateSalt.Hint", "The 'Private salt' for Instamojo payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.Description.Hint", "Enter info that will be shown to customers during checkout."),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.EnableSendEmail", "Enable send email"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.EnableSendEmail.Hint", "Check to enable send email by instamojo payment gateway."),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.EnableSendSMS", "Enable send SMS"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.EnableSendSMS.Hint", "Check to enable send SMS by instamojo payment gateway."),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.PhoneNumberRegex", "Phone number reg-ex"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.PhoneNumberRegex.Hint", "Enter regular expression for phone number format validation. Keep it empty if you want to skip it."),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.Instamojo.Configuration.Fields.UseSandbox.Hint", "Check to enable testing environment (sandbox)."),
            };

            return list;
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.Instamojo.Description");
        }

        #endregion
    }
}
