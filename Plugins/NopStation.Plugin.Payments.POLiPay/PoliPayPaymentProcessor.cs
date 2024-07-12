using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.POLiPay.Components;
using NopStation.Plugin.Payments.POLiPay.Factories;
using NopStation.Plugin.Payments.POLiPay.Models;

namespace NopStation.Plugin.Payments.POLiPay
{
    public class PoliPayPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IPolypayModelFactory _polipayFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPermissionService _permissionService;
        private readonly PoliPaySettings _poliPaySettings;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PoliPayPaymentProcessor(IPolypayModelFactory polipayFactory,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            PoliPaySettings poliPaySettings,
            INopStationCoreService nopStationCoreService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger logger,
            INotificationService notificationService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _polipayFactory = polipayFactory;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderService = orderService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _logger = logger;
            _notificationService = notificationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _permissionService = permissionService;
            _poliPaySettings = poliPaySettings;
            _nopStationCoreService = nopStationCoreService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending });
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var baseUrl = _poliPaySettings.UseSandbox ? PoliPayDefaults.SANDBOX_BASE_URL : PoliPayDefaults.BASE_URL;
            var resourceAddress = PoliPayDefaults.GET_URL_RESOURCE;
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            request = PoliPayHelper.AddHeaders(request, "POST", _poliPaySettings);

            var order = postProcessPaymentRequest.Order;
            var requestBody = new PaymentUrlRequest
            {
                Amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _workContext.GetWorkingCurrencyAsync()),
                CurrencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode,
                NotificationURL = $"{_webHelper.GetStoreLocation()}polipay/postpaymenthandler",
                FailureURL = $"{_webHelper.GetStoreLocation()}polipay/postpaymenthandler",
                SuccessURL = $"{_webHelper.GetStoreLocation()}polipay/postpaymenthandler",
                CancellationURL = $"{_webHelper.GetStoreLocation()}polipay/postpaymenthandler",
                MerchantHomepageURL = $"{_webHelper.GetStoreLocation()}",
                MerchantReference = order.Id.ToString(),
            };

            var json = JsonConvert.SerializeObject(requestBody);
            request.ContentLength = json.Length;
            using (var webStream = request.GetRequestStream())
            {
                using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                requestWriter.Write(json);
            }

            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var paymentUrlResponse = JsonConvert.DeserializeObject<PaymentUrlResponse>(response);

                if (paymentUrlResponse.Success)
                {
                    var transactionId = paymentUrlResponse.TransactionRefNo;
                    var redirectionUrl = paymentUrlResponse.NavigateURL;
                    order.AuthorizationTransactionId = transactionId;
                    await _orderService.UpdateOrderAsync(order);
                    _httpContextAccessor.HttpContext.Response.Redirect(redirectionUrl);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.PoliPay.RedirectionFailed"));
            }
        }

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            var currencyId = (await _workContext.GetCurrentCustomerAsync()).CurrencyId ?? 0;
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currencyId);
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();

            if (customerCurrency.CurrencyCode == "NZD" || customerCurrency.CurrencyCode == "AUD")
                return false;
            return true;
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _poliPaySettings.AdditionalFee, _poliPaySettings.AdditionalFeePercentage);
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
            result.AddError("Refund method not supported");
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
            result.AddError("Recurring method not supported");
            return Task.FromResult(result);
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring method not supported");
            return Task.FromResult(result);
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(true);
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPolipay/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentPolipayViewComponent);
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new PoliPayPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new PoliPayPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PoliPay.Menu.PoliPayPayment"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(PoliPayPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PoliPay.Menu.Configuration"),
                    Url = "~/Admin/PaymentPoliPay/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "PoliPay.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/polipay-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=polipay-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration", "POLi payment settings"),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Menu.PoliPayPayment", "POLi payment"),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.MerchantCode", "Merchant code"),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.MerchantCode.Hint", "Enter POLi pay merchant code"),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.AuthCode", "Auth code"),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.AuthCode.Hint", "Enter POLi pay auth code"),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.UseSandbox", "Use Sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.UseSandbox.Hint", "Determine whether to use the sandbox environment for testing purposes."),

                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),

                new KeyValuePair<string, string>("Admin.NopStation.PoliPay.Configuration.Suggestions", "<p>This payment method works by Polipay user interface to pay most securely. Accepted currencies are NZD and AUD.</p>"),

                new KeyValuePair<string, string>("NopStation.PoliPay.PaymentMethodDescription", "Pay using POLi Pay"),
                new KeyValuePair<string, string>("NopStation.PoliPay.RedirectionTip", "You will be redirected to POLi Pay payment gateway"),
                new KeyValuePair<string, string>("NopStation.PoliPay.RedirectionFailed", "Redirected to POLi Pay payment gateway is failed"),
            };

            return list;
        }

        public Task<string> GetPaymentMethodDescriptionAsync()
        {
            return Task.FromResult(_localizationService.GetResourceAsync("NopStation.PoliPay.PaymentMethodDescription").Result);
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        #endregion
    }
}
