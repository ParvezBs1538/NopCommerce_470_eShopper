using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
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
using NopStation.Plugin.Payments.AmazonPay.Components;
using NopStation.Plugin.Payments.AmazonPay.Services;

namespace NopStation.Plugin.Payments.AmazonPay
{
    public class AmazonPayPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly AmazonPaySettings _amazonPaySettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly AmazonManager _amazonManager;

        #endregion

        #region Ctor
        public AmazonPayPaymentProcessor(AmazonPaySettings amazonPaySettings,
            ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IHttpContextAccessor httpContextAccessor,
            IWorkContext workContext,
            AmazonManager amazonManager)
        {
            _amazonPaySettings = amazonPaySettings;
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _amazonManager = amazonManager;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => true;

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
        public bool SupportVoid => true;

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

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = false
            };
            return Task.FromResult(result);
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var url = $"{_webHelper.GetStoreLocation()}amazonpay/redirect/{postProcessPaymentRequest.Order.OrderGuid}";
            _httpContextAccessor.HttpContext.Response.Redirect(url);

            return Task.CompletedTask;
        }

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            var currency = await _workContext.GetWorkingCurrencyAsync();
            return !currency.ValidateCurrency();
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
               _amazonPaySettings.AdditionalFee, _amazonPaySettings.AdditionalFeePercentage);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            return Task.FromResult(result);
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            var refundResult = await _amazonManager.RefundAsync(refundPaymentRequest);
            if (refundResult.Success)
                result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
            else
            {
                if (!string.IsNullOrWhiteSpace(refundResult.RawResponse))
                    result.AddError(JObject.Parse(refundResult.RawResponse)["message"]?.ToString());
                else
                    result.AddError("Failed to refund");
            }

            return result;
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            return Task.FromResult(result);
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            return Task.FromResult(result);
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            //always success
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentAmazonPay/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(AmazonPayViewComponent);
        }

        public override async Task InstallAsync()
        {
            //settings
            var amazonPaySettings = new AmazonPaySettings
            {
                UseSandbox = true,
                DescriptionText = "<p>You will be redirected to Amazon Pay gateway.</p>",
                NoteToBuyer = "Thank you for your order",
                ButtonColor = "Gold"
            };
            await _settingService.SaveSettingAsync(amazonPaySettings);

            await this.InstallPluginAsync(new AmazonPayPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<AmazonPaySettings>();

            await this.UninstallPluginAsync(new AmazonPayPermissionProvider());
            await base.UninstallAsync();
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();
            return Task.FromResult<IList<string>>(warnings);
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return Task.FromResult(true);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.DescriptionText", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.DescriptionText.Hint", "Enter info that will be shown to customers during checkout"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.MerchantId", "Merchant ID / Seller ID"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.MerchantId.Hint", "Enter Amazon Pay merchant ID or seller ID."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.PrivateKey", "Private key"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.PrivateKey.Hint", "Enter Amazon Pay rivate key."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.PublicKeyId", "Public key ID"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.PublicKeyId.Hint", "Enter Amazon Pay public key ID."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.NoteToBuyer", "Note to buyer"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.NoteToBuyer.Hint", "Enter custom note from seller to buyer."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.ButtonColor", "Button color"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.ButtonColor.Hint", "Select Amazon Pay button color."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.StoreId", "Store ID / Client ID"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.StoreId.Hint", "Enter Amazon Pay store ID or client ID."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.Region", "Region"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.Region.Hint", "Select a region."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration.Fields.UseSandbox.Hint", "Check to enable testing environment (sandbox)."),

                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Menu.AmazonPay", "Amazon Pay"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPay.Configuration", "Amazon Pay settings"),

                new KeyValuePair<string, string>("NopStation.AmazonPay.PaymentMethodDescription", "Pay by Amazon Pay")
            };

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AmazonPay.Menu.AmazonPay"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(AmazonPayPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AmazonPay.Menu.Configuration"),
                    Url = "~/Admin/PaymentAmazonPay/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AmazonPay.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/stripe-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=stripe-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.AmazonPay.PaymentMethodDescription");
        }

        #endregion
    }
}
