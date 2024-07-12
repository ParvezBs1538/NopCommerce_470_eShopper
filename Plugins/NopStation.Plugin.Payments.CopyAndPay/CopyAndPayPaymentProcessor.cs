using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
using NopStation.Plugin.Payments.CopyAndPay.Services;

namespace NopStation.Plugin.Payments.CopyAndPay
{
    public class CopyAndPayPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly CopyAndPayPaymentSettings _cOPYandPayPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICopyAndPayServices _cOPYandPayServices;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public CopyAndPayPaymentProcessor(CopyAndPayPaymentSettings cOPYandPayPaymentSettings,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IHttpContextAccessor httpContextAccessor,
            ICopyAndPayServices cOPYandPayServices,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _cOPYandPayPaymentSettings = cOPYandPayPaymentSettings;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _cOPYandPayServices = cOPYandPayServices;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var orderId = postProcessPaymentRequest.Order.Id;
            var url = $"{_webHelper.GetStoreLocation()}copyandpay/payment/{orderId}";
            _httpContextAccessor.HttpContext.Response.Redirect(url);

            return Task.CompletedTask;
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                 _cOPYandPayPaymentSettings.AdditionalFee, _cOPYandPayPaymentSettings.AdditionalFeePercentage);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            var paidStatusRegex = new Regex(@"^(000\.000\.|000\.100\.1|000\.[36])");

            if (!_cOPYandPayServices.RefundPayment(refundPaymentRequest, out var responseData))
            {
                result.AddError("Failed");
            }
            else if (paidStatusRegex.Match(responseData.Result.Code).Success)
            {
                result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
            }
            else
            {
                result.AddError(responseData.Result.Description);
            }

            return Task.FromResult(result);
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

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/CopyAndPay/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return null;
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new CopyAndPayPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<CopyAndPayPaymentSettings>();
            await this.UninstallPluginAsync(new CopyAndPayPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.CopyAndPay.PaymentMethodDescription");
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.CopyAndPay.Menu.CopyAndPay"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(CopyAndPayPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CopyAndPay.Menu.Configuration"),
                    Url = "~/Admin/CopyAndPay/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CopyAndPay"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/copyandpay-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=copyandpay-payment",
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
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration", "COPYandPay settings"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Menu.CopyAndPay", "CopyAndPay"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Instructions", "This payment method works by CopyAndPay user interface to pay most securely."),

                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.AdditionalFee.Hint", "The additional fee."),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.EntityId", "Entity id"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.EntityId.Hint", "Enter valid entity id."),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.MadaEntityId", "Mada entity id"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.MadaEntityId.Hint", "Enter valid mada entity id."),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.TestMode", "Test Mode"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.TestMode.Hint", "Enter valid test mode (e.g. EXTERNAL/ INTERNAL)."),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.AuthorizationKey", "Authorization key"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.AuthorizationKey.Hint", "Enter authorization key with bearer."),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.APIUrl", "API url"),
                new KeyValuePair<string, string>("Admin.NopStation.CopyAndPay.Configuration.Fields.APIUrl.Hint", "Enter valid test or production API url."),

                new KeyValuePair<string, string>("NopStation.CopyAndPay.Payment.Initialization.Failed", "Payment initialization failed!"),
                new KeyValuePair<string, string>("NopStation.CopyAndPay.PaymentMethodDescription", "Pay by Hyper COPYandPay credit / debit card"),

                new KeyValuePair<string, string>("NopStation.CopyAndPay.Checkout.PaymentTitle", "Pay with COPYandPay"),
                new KeyValuePair<string, string>("NopStation.CopyAndPay.Checkout.OrderSuccessfullyPlaced", "Your order has been successfully placed. Pay your order using COPYandPay.")
            };

            return list;
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => true;

        #endregion
    }
}