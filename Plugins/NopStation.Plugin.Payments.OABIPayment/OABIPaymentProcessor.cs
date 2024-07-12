using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.OABIPayment.Components;
using NopStation.Plugin.Payments.OABIPayment.Helpers;
using NopStation.Plugin.Payments.OABIPayment.Services;

namespace NopStation.Plugin.Payments.OABIPayment
{
    public class OABIPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly OABIPaymentSettings _oabIPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IOABIPaymentService _oabIPaymentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public OABIPaymentProcessor(ISettingService settingService,
            IWebHelper webHelper,
            OABIPaymentSettings paykeeperPaymentSettings,
            ILocalizationService localizationService,
            IOABIPaymentService paykeeperWebRequest,
            IHttpContextAccessor httpContextAccessor,
            ILogger logger,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _settingService = settingService;
            _webHelper = webHelper;
            _oabIPaymentSettings = paykeeperPaymentSettings;
            _localizationService = localizationService;
            _oabIPaymentService = paykeeperWebRequest;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _orderTotalCalculationService = orderTotalCalculationService;
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
            return $"{_webHelper.GetStoreLocation()}Admin/OABIPayment/Configure";
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            try
            {
                var link = await _oabIPaymentService.GetLink(postProcessPaymentRequest);

                if (link != null)
                {
                    if (_httpContextAccessor.HttpContext != null)
                    {
                        await _logger.InformationAsync($"OABIPayment payment link generated for order id {postProcessPaymentRequest.Order.Id}");
                        _httpContextAccessor.HttpContext.Response.Redirect(link);
                    }
                }
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
            }
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country

            var notConfigured = !Helper.IsConfigured(_oabIPaymentSettings);
            return Task.FromResult(notConfigured);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            var result = await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _oabIPaymentSettings.AdditionalFee, _oabIPaymentSettings.AdditionalFeeInPercentage);
            return result;
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            result.AddError("Capture method not supported");

            return Task.FromResult(result);
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund not supported");

            return Task.FromResult(result);
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

            //OABIPayment is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return Task.FromResult(false);

            //let's ensure that at least 5 seconds passed after order is placed
            return Task.FromResult(!((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5));
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new OABIPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //locales
            await _settingService.DeleteSettingAsync<OABIPaymentSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("NopStation.Payments.OABIPayment");
            await _localizationService.DeleteLocaleResourcesAsync("Admin.NopStation.Plugins");

            await this.UninstallPluginAsync(new OABIPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public Type GetPublicViewComponent()
        {
            return typeof(OABIPaymentViewComponent);
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.Payments.OABIPayment.PaymentMethodDescription");
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Payments.OABIPayment.Menu.OABIPayment"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(OABIPaymentPermissionProvider.ManageConfiguration))
            {
                var configuration = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Payments.OABIPayment.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/OABIPayment/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "OABIPayment.Configuration"
                };
                menuItem.ChildNodes.Add(configuration);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/oab-ipay-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=oab-ipay-payment",
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
                new KeyValuePair<string, string>("NopStation.Payments.OABIPayment.RedirectionTip", "You will be redirected to OABIPayment site to complete the order."),
                new KeyValuePair<string, string>("NopStation.Payments.OABIPayment.CurrencyNotSupported", "Your selected currency is not supported. Please select INR or OMR for use OAB I Payment gatware."),
                new KeyValuePair<string, string>("NopStation.Payments.OABIPayment.PaymentMethodDescription", "You will be redirected to OABIPayment site to complete the order."),

                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.TranPortaId", "Tranportal id"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.TranPortaId.Hint", "Set the tranportal id of your account"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.TranPortaPassword", "Tranportal Password"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.TranPortaPassword.Hint", "Set the tranportal password of your account"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.AuthorizeOnly", "Authorize only"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.AuthorizeOnly.Hint", "Authorize only?"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.AdditionalFeeInPercentage", "Enable additional fee in percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.AdditionalFeeInPercentage.Hint", "Enable additional fee in percentage?"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.ResourceKey", "Resource key"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.ResourceKey.Hint", "Set the resource key of your account"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.Menu.OABIPayment", "OAB iPAY"),
                new KeyValuePair<string, string>("Admin.NopStation.Payments.OABIPayment.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.OABIPayment.Menu.TransactionMode", "Transaction mode"),
                new KeyValuePair<string, string>("Admin.NopStation.OABIPayment.Menu.TransactionMode.Hint", "Select transaction mode."),
                new KeyValuePair<string, string>("Admin.NopStation.OABIPayment.Configuration", "OAB iPAY configuration")
            };
            return list;
        }

        #endregion

        #region Properies

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => true;

        #endregion
    }
}
