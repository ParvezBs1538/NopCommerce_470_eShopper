using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.AamarPay.Components;
using NopStation.Plugin.Payments.AamarPay.Services;

namespace NopStation.Plugin.Payments.AamarPay;

public class AamarPayPaymentPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
{
    #region Fields

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWorkContext _workContext;
    private readonly IWebHelper _webHelper;
    private readonly ICurrencyService _currencyService;
    private readonly WidgetSettings _widgetSettings;
    private readonly IAamarPayPaymentService _aamarPayPaymentService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IPermissionService _permissionService;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly AamarPayPaymentSettings _aamarPayPaymentSettings;

    #endregion

    #region Ctor

    public AamarPayPaymentPaymentProcessor(IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService,
        ISettingService settingService,
        IWorkContext workContext,
        IWebHelper webHelper,
        ICurrencyService currencyService,
        WidgetSettings widgetSettings,
        IAamarPayPaymentService aamarPayPaymentService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IPermissionService permissionService,
        INopStationCoreService nopStationCoreService,
        AamarPayPaymentSettings aamarPayPaymentSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;
        _settingService = settingService;
        _workContext = workContext;
        _webHelper = webHelper;
        _currencyService = currencyService;
        _widgetSettings = widgetSettings;
        _aamarPayPaymentService = aamarPayPaymentService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _permissionService = permissionService;
        _nopStationCoreService = nopStationCoreService;
        _aamarPayPaymentSettings = aamarPayPaymentSettings;
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
        var paymentUrlRequest = await _aamarPayPaymentService.AamarPayPaymentInitAsync(order);

        if (paymentUrlRequest == null || string.IsNullOrEmpty(paymentUrlRequest.PaymentUrl))
        {
            _httpContextAccessor.HttpContext.Response.Redirect($"{_webHelper.GetStoreLocation()}OrderDetails/{order.Id}");
            return;
        }

        _httpContextAccessor.HttpContext.Response.Redirect(paymentUrlRequest.PaymentUrl);
    }

    public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        if (string.IsNullOrEmpty(_aamarPayPaymentSettings.SignatureKey) || string.IsNullOrEmpty(_aamarPayPaymentSettings.MerchantId))
            return true;

        var currentCustomerCurrencyId = (await _workContext.GetCurrentCustomerAsync()).CurrencyId;
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currentCustomerCurrencyId ?? 0);
        var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();

        if (!(customerCurrency.CurrencyCode == "BDT" || customerCurrency.CurrencyCode == "USD"))
            return true;

        return false;
    }

    public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
           _aamarPayPaymentSettings.AdditionalFee, _aamarPayPaymentSettings.AdditionalFeePercentage);
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

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/AamarPayPayment/Configure";
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { });
    }

    public override async Task InstallAsync()
    {
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(AamarPayPaymentDefaults.PLUGIN_SYSTEM_NAME))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(AamarPayPaymentDefaults.PLUGIN_SYSTEM_NAME);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await this.InstallPluginAsync(new AamarPayPaymentPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<AamarPayPaymentSettings>();

        if (_widgetSettings.ActiveWidgetSystemNames.Contains(AamarPayPaymentDefaults.PLUGIN_SYSTEM_NAME))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(AamarPayPaymentDefaults.PLUGIN_SYSTEM_NAME);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await this.UninstallPluginAsync(new AamarPayPaymentPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("NopStation.AamarPay.PaymentMethodDescription");
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.AamarPay.Menu.AamarPay"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(AamarPayPaymentPermissionProvider.ManageConfiguration))
        {
            var categoryIconItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AamarPay.Menu.Configuration"),
                Url = "~/Admin/AamarPayPayment/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "AamarPay"
            };
            menuItem.ChildNodes.Add(categoryIconItem);
        }
        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/aamarpay-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=aamarpay-payment",
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
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Menu.AamarPay", "AamarPay"),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Menu.Configuration", "Configuration"),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Configuration", "AamarPay settings"),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Configuration.Saved", "AamarPay settings saved successfully"),

            new KeyValuePair<string, string>("NopStation.AamarPay.Fields.RedirectionTip", "You will be redirected to AamarPay website"),
            new KeyValuePair<string, string>("NopStation.AamarPay.PaymentMethodDescription", "Pay by AamarPay"),

            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.UseSandbox", "Use sandbox"),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.UseSandbox.Hint", "Check to enable testing environment (sandbox)."),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.MerchantId", "Merchant id"),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.MerchantId.Hint", "Enter merchant id provided by aamarpay."),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.SignatureKey", "Signature key"),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.SignatureKey.Hint", "Enter the aamarpay signature key"),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.AdditionalFee", "Additional fee"),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
            new KeyValuePair<string, string>("Admin.NopStation.AamarPay.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),

            new KeyValuePair<string, string>("NopStation.Afterpay.Message.Notpaid", "Your order payment is not completed yet")
        };

        return list;
    }

    public Type GetPublicViewComponent()
    {
        return typeof(AamarPayPaymentViewComponent);
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return null;
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

    public bool HideInWidgetList => false;

    #endregion
}
