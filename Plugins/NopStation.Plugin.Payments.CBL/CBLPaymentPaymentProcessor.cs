using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.CBL.Components;
using NopStation.Plugin.Payments.CBL.Services;

namespace NopStation.Plugin.Payments.CBL
{
    public class CBLPaymentPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ICBLPaymentService _cBLRequestService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CBLPaymentPaymentProcessor(IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderService orderService,
            ISettingService settingService,
            IWebHelper webHelper,
            ICBLPaymentService cBLRequestService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderService = orderService;
            _settingService = settingService;
            _webHelper = webHelper;
            _cBLRequestService = cBLRequestService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending });
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var redirectUrl = await _genericAttributeService.GetAttributeAsync<string>(order, CBLPaymentDefaults.RedirectUrlKey, order.StoreId);
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                _httpContextAccessor.HttpContext.Response.Redirect(redirectUrl);
                return;
            }
            var paymentUrlRequest = await _cBLRequestService.GeneratePaymentUrlRequestAsync(order);
            var paymentUrlResponse = await _cBLRequestService.GetResponseAsync(paymentUrlRequest);

            if (paymentUrlResponse == null)
            {
                _httpContextAccessor.HttpContext.Response.Redirect($"{_webHelper.GetStoreLocation()}OrderDetails/{order.Id}");
                return;
            }
            paymentUrlResponse.RetuenUrl = paymentUrlResponse.RetuenUrl + "&CardNo";
            order.AuthorizationTransactionId = paymentUrlResponse.TransactionId;
            order.AuthorizationTransactionCode = paymentUrlResponse.GatewayTransactionId;
            await _genericAttributeService.SaveAttributeAsync(order, CBLPaymentDefaults.RedirectUrlKey, paymentUrlResponse.RetuenUrl, order.StoreId);
            await _orderService.UpdateOrderAsync(order);
            _httpContextAccessor.HttpContext.Response.Redirect(paymentUrlResponse.RetuenUrl);
        }

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            var currency = await _workContext.GetWorkingCurrencyAsync();
            if (currency == null || string.Equals(currency.CurrencyCode, "BDT", StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }

        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(0m);
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

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentCBLViewComponent);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/CBLPayment/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new CBLPaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<CBLPaymentSettings>();

            await this.UninstallPluginAsync(new CBLPaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.CBL.PaymentMethodDescription");
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.CBL.Menu.CBL"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(CBLPaymentPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CBL.Menu.Configuration"),
                    Url = "~/Admin/CBLPayment/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CBL"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/city-bank-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=city-bank-payment",
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
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Menu.CBL", "City bank payment"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Configuration", "City bank payment settings"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Configuration.Saved", "City bank payment settings saved successfully"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Instructions", "This payment method works by City bank user interface to pay most securely."),

                new KeyValuePair<string, string>("NopStation.CBL.Fields.RedirectionTip", "You will be redirected to city bank payment website"),
                new KeyValuePair<string, string>("NopStation.CBL.PaymentMethodDescription", "Pay by city bank payment"),

                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.UseSandbox.Hint", "Check to enable testing environment (sandbox)."),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.Debug", "Debug"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.Debug.Hint", "Debug mode"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.MerchantId", "Merchant id"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.MerchantId.Hint", "Please use valid merchant Id"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.MerchantUsername", "Merchant Username"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.MerchantUsername.Hint", "Please use valid merchant username"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.MerchantPassword", "Merchant password"),
                new KeyValuePair<string, string>("Admin.NopStation.CBL.Fields.MerchantPassword.Hint", "Please use valid merchant password"),

                new KeyValuePair<string, string>("NopStation.CBL.Message.Notpaid", "Your order payment is not completed yet")
            };

            return list;
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        public bool HideInWidgetList => false;

        #endregion
    }
}
