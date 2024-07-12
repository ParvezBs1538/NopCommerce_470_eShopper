using System;
using System.Collections.Generic;
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
using NopStation.Plugin.Payments.Razorpay.Components;
using NopStation.Plugin.Payments.Razorpay.Services;

namespace NopStation.Plugin.Payments.Razorpay
{
    public class RazorpayPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly RazorpayPaymentSettings _razorpayPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly RazorpayManager _razorpayManager;
        private readonly ISettingService _settingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public RazorpayPaymentProcessor(IWebHelper webHelper,
            RazorpayPaymentSettings razorpayPaymentSettings,
            ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            RazorpayManager razorpayManager,
            ISettingService settingService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _webHelper = webHelper;
            _razorpayPaymentSettings = razorpayPaymentSettings;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _razorpayManager = razorpayManager;
            _settingService = settingService;
            _orderTotalCalculationService = orderTotalCalculationService;
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentRazorpay/Configure";
        }

        public override async Task InstallAsync()
        {
            //settings
            var razorpayPaymentSettings = new RazorpayPaymentSettings
            {
                Description = "<p>You will be redirected to Razorpay gateway.</p>"
            };
            await _settingService.SaveSettingAsync(razorpayPaymentSettings);

            await this.InstallPluginAsync(new RazorpayPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new RazorpayPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Razorpay.RecurringNotSupported"));
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
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Razorpay.CaptureNotSupported"));
            return result;
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _razorpayPaymentSettings.AdditionalFee, _razorpayPaymentSettings.AdditionalFeePercentage);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var url = await _razorpayManager.GetPaymentLinkAsync(order);

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
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Razorpay.RecurringNotSupported"));
            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return await _razorpayManager.RefundAsync(refundPaymentRequest);
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError(await _localizationService.GetResourceAsync("NopStation.Razorpay.VoidNotSupported"));
            return result;
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public Type GetPublicViewComponent()
        {
            return typeof(RazorpayViewComponent);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Razorpay.Menu.Razorpay"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(RazorpayPaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Razorpay.Menu.Configuration"),
                    Url = "~/Admin/PaymentRazorpay/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Razorpay.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/razorpay-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=razorpay-payment",
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
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration", "Razorpay payment settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Menu.Razorpay", "Razorpay"),
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("NopStation.Razorpay.CaptureNotSupported", "Capture method not supported"),
                new KeyValuePair<string, string>("NopStation.Razorpay.RefundNotSupported", "Refund method not supported"),
                new KeyValuePair<string, string>("NopStation.Razorpay.VoidNotSupported", "Void method not supported"),
                new KeyValuePair<string, string>("NopStation.Razorpay.RecurringNotSupported", "Recurring method not supported"),

                new KeyValuePair<string, string>("NopStation.Razorpay.Description", "Pay by Razorpay"),

                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.KeyId", "Key ID"),
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.KeySecret", "Key secret"),
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.Description", "Description"),

                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),

                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.KeyId.Hint", "The 'Key ID' for Razorpay payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.KeySecret.Hint", "The 'Key secret' for Razorpay payment."),
                new KeyValuePair<string, string>("Admin.NopStation.Razorpay.Configuration.Fields.Description.Hint", "Enter info that will be shown to customers during checkout.")
            };

            return list;
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.Razorpay.Description");
        }

        #endregion
    }
}
