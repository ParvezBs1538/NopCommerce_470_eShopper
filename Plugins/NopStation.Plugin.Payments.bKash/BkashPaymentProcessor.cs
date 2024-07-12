using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Payments.bKash
{
    public class BkashPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly BkashPaymentSettings _bkashSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public BkashPaymentProcessor(IWebHelper webHelper,
            BkashPaymentSettings bkashSettings,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _webHelper = webHelper;
            _bkashSettings = bkashSettings;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

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
        /// Skip payment info during checkout
        /// </summary>
        public bool SkipPaymentInfo => true;

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/BkashPayment/Configure";
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending });
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var url = $"{_webHelper.GetStoreLocation()}checkout/bkashpay/{postProcessPaymentRequest.Order.Id}";
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
                _bkashSettings.AdditionalFee, _bkashSettings.AdditionalFeePercentage);
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("Admin.NopStation.bKash.CaptureNotSupported"));
            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("Admin.NopStation.bKash.RefundNotSupported"));
            return result;
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("Admin.NopStation.bKash.VoidNotSupported"));
            return result;
        }

        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("Admin.NopStation.bKash.RecurringNotSupported"));
            return result;
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("Admin.NopStation.bKash.RecurringNotSupported"));
            return result;
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(false);
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new BkashPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new BkashPermissionProvider());
            await base.UninstallAsync();
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Type GetPublicViewComponent()
        {
            throw new NotImplementedException();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.bKash.Menu.bKashPayment"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(BkashPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.bKash.Menu.Configuration"),
                    Url = "~/Admin/BkashPayment/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "bKashPayment.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/bkash-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=bkash-payment",
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
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration", "bKash payment settings"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Menu.bKashPayment", "bKash payment"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.bKash.CaptureNotSupported", "Capture method not supported"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.RefundNotSupported", "Refund method not supported"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.VoidNotSupported", "Void method not supported"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.RecurringNotSupported", "Payment recurring not supported"),

                new KeyValuePair<string, string>("NopStation.bKash.PaymentDescription", "Pay with bKash"),
                new KeyValuePair<string, string>("NopStation.bKash.Checkout.PayWithBbKash", "Pay with bKash"),
                new KeyValuePair<string, string>("NopStation.bKash.Checkout.PaymentTitle", "Pay with bKash"),
                new KeyValuePair<string, string>("NopStation.bKash.Checkout.OrderSuccessfullyPlaced", "Your order has been successfully placed. Click 'Pay with bKash' to continue payment."),

                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.AppKey", "App key"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.AppKey.Hint", "The 'App key' for bKash payment."),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.AppSecret", "App secret"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.AppSecret.Hint", "The 'App secret' for bKash payment."),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.Username", "Username"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.Username.Hint", "The 'Username' for bKash payment."),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.Password", "Password"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.Password.Hint", "The 'Password' for bKash payment."),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.Description", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.Description.Hint", "The 'Description' for bKash payment."),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.UseSandbox.Hint", "Check if application is in development or test mode."),

                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.CreateTokenSandboxUrl", "Create token sandbox url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.CreateTokenSandboxUrl.Hint", "Provide create token sandbox url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.CreateTokenLiveUrl", "Create token live url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.CreateTokenLiveUrl.Hint", "Provide create token live url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.CreatePaymentSandboxUrl", "Create payment sandbox url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.CreatePaymentSandboxUrl.Hint", "Provide create payment sandbox url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.CreatePaymentLiveUrl", "Create payment live url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.CreatePaymentLiveUrl.Hint", "Provide create payment live url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.ExecutePaymentSandboxUrl", "Execute payment sandbox url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.ExecutePaymentSandboxUrl.Hint", "Provide execute payment sandbox url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.ExecutePaymentLiveUrl", "Execute payment live url"),
                new KeyValuePair<string, string>("Admin.NopStation.bKash.Configuration.Fields.ExecutePaymentLiveUrl.Hint", "Provide execute payment live url")
            };

            return list;
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.bKash.PaymentDescription");
        }

        #endregion
    }
}
