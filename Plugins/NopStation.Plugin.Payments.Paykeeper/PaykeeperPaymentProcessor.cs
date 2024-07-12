using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.Paykeeper.Components;
using NopStation.Plugin.Payments.Paykeeper.Services;

namespace NopStation.Plugin.Payments.Paykeeper
{
    public class PaykeeperPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly PaykeeperPaymentSettings _paykeeperPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPaykeeperWebRequest _paykeeperWebRequest;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPaymentService _paymentService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;


        #endregion

        #region Ctor

        public PaykeeperPaymentProcessor(ISettingService settingService,
            IWebHelper webHelper,
            PaykeeperPaymentSettings paykeeperPaymentSettings,
            ILocalizationService localizationService,
            IPaykeeperWebRequest paykeeperWebRequest,
            IHttpContextAccessor httpContextAccessor,
            IPaymentService paymentService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _paykeeperPaymentSettings = paykeeperPaymentSettings;
            _localizationService = localizationService;
            _paykeeperWebRequest = paykeeperWebRequest;
            _httpContextAccessor = httpContextAccessor;
            _paymentService = paymentService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _logger = logger;
            _orderTotalCalculationService = orderTotalCalculationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending };

            return Task.FromResult(result);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();

            return Task.FromResult(paymentInfo);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPaykeeper/Configure";
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            try
            {
                var data = await _paykeeperWebRequest.GetInvoiceInformationAsync(postProcessPaymentRequest);
                _httpContextAccessor.HttpContext.Response.Redirect(data.InvoiceUrl);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
            }
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            var result = await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _paykeeperPaymentSettings.AdditionalFee, _paykeeperPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");

            return Task.FromResult(result);
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            //get the primary store currency
            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            if (currency == null)
                throw new NopException("Primary store currency cannot be loaded");

            var error = _paykeeperWebRequest.Refund(refundPaymentRequest);

            if (!string.IsNullOrEmpty(error))
                return new RefundPaymentResult { Errors = new[] { error } };

            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
            };
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");

            return Task.FromResult(result);
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");

            return Task.FromResult(result);
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");

            return Task.FromResult(result);
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return Task.FromResult(false);

            //let's ensure that at least 1 minute passed after order is placed
            return Task.FromResult(!((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1));
        }

        public override async Task InstallAsync()
        {
            var settings = new PaykeeperPaymentSettings
            {
                GatewayUrl = "",
                SecretWord = "",
                AdditionalFee = 0,
                DescriptionText = "You will be redirected to Paykeeper site to complete the order."
            };

            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new PaykeeperPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //locales
            await _settingService.DeleteSettingAsync<PaykeeperPaymentSettings>();
            await this.UninstallPluginAsync(new PaykeeperPermissionProvider());
            await base.UninstallAsync();
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaykeeperPaymentViewComponent);
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.Payments.Paykeeper.PaymentMethodDescription");
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>(new Dictionary<string, string>
            {
                ["NopStation.Payments.Paykeeper.PaymentMethodDescription"] = "Pay with Paykeeper",

                ["Admin.NopStation.Paykeeper.Configuration.Fields.GatewayUrl"] = "Gateway URL",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.GatewayUrl.Hint"] = "Enter gateway URL.",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.SecretWord"] = "Secret word",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.SecretWord.Hint"] = "Copy the secret word from your personal account under the 'Getting information about payment' section",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.AuthorizeOnly"] = "Authorize only",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.AuthorizeOnly.Hint"] = "Authorize only?",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.AdditionalFee"] = "Additional fee",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.Password"] = "Password",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.Password.Hint"] = "Set the password.",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.Login"] = "Login",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.Login.Hint"] = "Set the login.",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.DescriptionText"] = "Description",
                ["Admin.NopStation.Paykeeper.Configuration.Fields.DescriptionText.Hint"] = "Enter info that will be shown to customers during checkout.",
                ["Admin.NopStation.Paykeeper.Configuration"] = "Paykeeper settings",

                ["Admin.NopStation.Paykeeper.Menu.Paykeeper"] = "Paykeeper",
                ["Admin.NopStation.Paykeeper.Menu.Configuration"] = "Configuration"
            });

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {

            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Paykeeper.Menu.Paykeeper"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(PaykeeperPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Paykeeper.Menu.Configuration"),
                    Url = "~/Admin/PaymentPaykeeper/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Paykeeper.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/paykeeper-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=paykeeper-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
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
    }
}
