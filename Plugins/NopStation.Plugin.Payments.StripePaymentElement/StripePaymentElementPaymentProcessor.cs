using System;
using System.Collections.Generic;
using System.Linq;
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
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.StripePaymentElement.Components;
using NopStation.Plugin.Payments.StripePaymentElement.Models;
using NopStation.Plugin.Payments.StripePaymentElement.Services;
using Stripe;

namespace NopStation.Plugin.Payments.StripePaymentElement;

public class StripePaymentElementPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin, IWidgetPlugin
{
    #region Fields

    private readonly StripePaymentElementSettings _stripePaymentElementSettings;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger _logger;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly ICurrencyService _currencyService;
    private readonly IWorkContext _workContext;
    private readonly IOrderService _orderService;
    private readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public StripePaymentElementPaymentProcessor(StripePaymentElementSettings stripePaymentElementSettings,
        ISettingService settingService,
        ILocalizationService localizationService,
        IWebHelper webHelper,
        INopStationCoreService nopStationCoreService,
        IPermissionService permissionService,
        ILogger logger,
        IOrderTotalCalculationService orderTotalCalculationService,
        IOrderProcessingService orderProcessingService,
        ICurrencyService currencyService,
        IWorkContext workContext,
        IOrderService orderService,
        WidgetSettings widgetSettings)
    {
        _stripePaymentElementSettings = stripePaymentElementSettings;
        _settingService = settingService;
        _localizationService = localizationService;
        _webHelper = webHelper;
        _nopStationCoreService = nopStationCoreService;
        _permissionService = permissionService;
        _logger = logger;
        _orderTotalCalculationService = orderTotalCalculationService;
        _orderProcessingService = orderProcessingService;
        _currencyService = currencyService;
        _workContext = workContext;
        _orderService = orderService;
        _widgetSettings = widgetSettings;
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
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

    /// <summary>
    /// Gets a value indicating whether we should display a payment information page for this plugin
    /// </summary>
    public bool SkipPaymentInfo => false;

    public bool HideInWidgetList => false;

    #endregion

    #region Methods

    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        var processPaymentResult = new ProcessPaymentResult();

        if (processPaymentRequest.CustomValues.TryGetValue(StripeDefaults.PaymentIntentId, out var intentId))
        {
            var service = new PaymentIntentService(new StripeClient(apiKey: _stripePaymentElementSettings.SecretKey));
            var paymentIntent = await service.GetAsync(processPaymentRequest.CustomValues[StripeDefaults.PaymentIntentId].ToString());

            if (paymentIntent.Status == PaymentIntentStatus.requires_capture.ToString())
            {
                await service.UpdateAsync(paymentIntent.Id, new PaymentIntentUpdateOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["OrderGuid"] = processPaymentRequest.OrderGuid.ToString()
                    }
                });

                var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(processPaymentRequest.OrderTotal, await _workContext.GetWorkingCurrencyAsync());
                if ((int)(orderTotal * 100) <= paymentIntent.Amount)
                {
                    processPaymentResult.NewPaymentStatus = PaymentStatus.Pending;
                    processPaymentResult.AuthorizationTransactionId = processPaymentRequest.CustomValues[StripeDefaults.PaymentIntentId].ToString();
                    processPaymentRequest.CustomValues.Remove(StripeDefaults.PaymentIntentId);

                    return processPaymentResult;
                }
            }
        }

        processPaymentResult.Errors.Add("Payment intent id not found");
        return processPaymentResult;
    }

    public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
    {
        var order = postProcessPaymentRequest.Order;
        var paymentIntentId = order.AuthorizationTransactionId;

        var service = new PaymentIntentService(new StripeClient(apiKey: _stripePaymentElementSettings.SecretKey));

        var paymentIntent = await service.GetAsync(paymentIntentId);

        if (paymentIntent == null || paymentIntent.Status != PaymentIntentStatus.requires_capture.ToString())
            return;

        await service.UpdateAsync(paymentIntent.Id, new PaymentIntentUpdateOptions
        {
            Metadata = new Dictionary<string, string>
            {
                ["OrderGuid"] = order.OrderGuid.ToString(),
                ["OrderId"] = order.Id.ToString()
            }
        });

        await _orderService.InsertOrderNoteAsync(new()
        {
            CreatedOnUtc = DateTime.UtcNow,
            DisplayToCustomer = false,
            OrderId = order.Id,
            Note = "Stripe payment info:\n" + paymentIntent.RawJObject.ToString(Newtonsoft.Json.Formatting.None)
        });

        if (_stripePaymentElementSettings.TransactionMode == TransactionMode.AuthorizeAndCapture)
        {
            var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _workContext.GetWorkingCurrencyAsync());
            var options = new PaymentIntentCaptureOptions
            {
                AmountToCapture = (long)(orderTotal * 100),
            };
            var paymentIntentPaid = await service.CaptureAsync(paymentIntentId, options);

            if (paymentIntentPaid != null &&
                paymentIntentPaid.Status == PaymentIntentStatus.succeeded.ToString() &&
                _orderProcessingService.CanMarkOrderAsPaid(order))
            {
                order.AuthorizationTransactionId = paymentIntentPaid.Id;
                order.CaptureTransactionResult = paymentIntentPaid.Status;
                order.CaptureTransactionId = paymentIntentPaid.Charges.Data.FirstOrDefault()?.Id;

                await _orderProcessingService.MarkOrderAsPaidAsync(order);
            }
        }
        else if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
        {
            order.AuthorizationTransactionResult = paymentIntent.Status;
            order.AuthorizationTransactionId = paymentIntentId;

            await _orderProcessingService.MarkAsAuthorizedAsync(order);
        }

        order.CardType = paymentIntent.Charges.Data.FirstOrDefault()?.PaymentMethodDetails?.Card?.Brand;
        await _orderService.UpdateOrderAsync(order);
    }

    public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        return Task.FromResult(!StripePaymentElementManager.IsConfigured(_stripePaymentElementSettings));
    }

    public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
            _stripePaymentElementSettings.AdditionalFee, _stripePaymentElementSettings.AdditionalFeePercentage);
    }

    public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        var result = new CapturePaymentResult();

        try
        {
            var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(capturePaymentRequest.Order.OrderTotal,
                await _workContext.GetWorkingCurrencyAsync());

            var options = new PaymentIntentCaptureOptions
            {
                AmountToCapture = (int)(orderTotal * 100),
            };

            var service = new PaymentIntentService(new StripeClient(apiKey: _stripePaymentElementSettings.SecretKey));
            var paymentIntent = await service.CaptureAsync(capturePaymentRequest.Order.AuthorizationTransactionId, options);

            await _orderService.InsertOrderNoteAsync(new()
            {
                CreatedOnUtc = DateTime.UtcNow,
                DisplayToCustomer = false,
                OrderId = capturePaymentRequest.Order.Id,
                Note = "Stripe payment info:\n" + paymentIntent.RawJObject.ToString(Newtonsoft.Json.Formatting.None)
            });

            if (paymentIntent.Status == PaymentIntentStatus.succeeded.ToString())
            {
                result.NewPaymentStatus = PaymentStatus.Paid;
                result.CaptureTransactionId = paymentIntent.Charges.Data.FirstOrDefault()?.Id;
                result.CaptureTransactionResult = paymentIntent.Status;
            }
            else
                result.AddError("Capture request failed");

            return result;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(ex.Message, ex);
            result.AddError("Capture request failed");
            return result;
        }
    }

    public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        var result = new RefundPaymentResult();
        var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(refundPaymentRequest.Order.OrderTotal, await _workContext.GetWorkingCurrencyAsync());
        var amountToRefund = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(refundPaymentRequest.AmountToRefund, await _workContext.GetWorkingCurrencyAsync());
        var refundPaymentAmount = refundPaymentRequest.IsPartialRefund ? amountToRefund : orderTotal;

        var options = new RefundCreateOptions
        {
            Amount = (long)(refundPaymentAmount * 100),
            Reason = RefundReasons.RequestedByCustomer,
            PaymentIntent = refundPaymentRequest.Order.AuthorizationTransactionId
        };

        var service = new RefundService(new StripeClient(apiKey: _stripePaymentElementSettings.SecretKey));
        var refund = service.Create(options);

        await _orderService.InsertOrderNoteAsync(new()
        {
            CreatedOnUtc = DateTime.UtcNow,
            DisplayToCustomer = false,
            OrderId = refundPaymentRequest.Order.Id,
            Note = "Stripe payment info:\n" + refund.RawJObject.ToString(Newtonsoft.Json.Formatting.None)
        });

        if (refund.Status == PaymentIntentStatus.succeeded.ToString() && await _orderProcessingService.CanRefundAsync(refundPaymentRequest.Order))
        {
            result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
        }
        else
        {
            result.AddError("Failed");
        }
        return await Task.FromResult(result);
    }

    public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        var result = new VoidPaymentResult();
        var service = new PaymentIntentService(new StripeClient(apiKey: _stripePaymentElementSettings.SecretKey));
        var paymentIntent = await service.CancelAsync(voidPaymentRequest.Order.AuthorizationTransactionId);

        await _orderService.InsertOrderNoteAsync(new()
        {
            CreatedOnUtc = DateTime.UtcNow,
            DisplayToCustomer = false,
            OrderId = voidPaymentRequest.Order.Id,
            Note = "Stripe payment info:\n" + paymentIntent.RawJObject.ToString(Newtonsoft.Json.Formatting.None)
        });

        if (paymentIntent != null && paymentIntent.Status == PaymentIntentStatus.canceled.ToString()
            && await _orderProcessingService.CanVoidAsync(voidPaymentRequest.Order))
        {
            result.NewPaymentStatus = PaymentStatus.Voided;
        }
        else
            result.AddError("Void request failed");

        return result;
    }

    public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        var result = new ProcessPaymentResult();

        switch (_stripePaymentElementSettings.TransactionMode)
        {
            case TransactionMode.Authorize:
                result.NewPaymentStatus = PaymentStatus.Authorized;
                break;
            case TransactionMode.AuthorizeAndCapture:
                result.NewPaymentStatus = PaymentStatus.Paid;
                break;
            default:
                result.AddError("Not supported transaction type");
                break;
        }
        return Task.FromResult(result);
    }

    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        //always success
        return Task.FromResult(new CancelRecurringPaymentResult());
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/StripePaymentElement/Configure";
    }

    public override async Task InstallAsync()
    {
        //settings
        var settings = new StripePaymentElementSettings
        {
            TransactionMode = TransactionMode.AuthorizeAndCapture,
            Theme = StripeDefaults.Themes[0],
            Layout = StripeDefaults.Layouts[0]
        };
        await _settingService.SaveSettingAsync(settings);

        await this.InstallPluginAsync(new StripePaymentElementsPermissionProvider());

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(StripeDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(StripeDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<StripePaymentElementSettings>();
        await this.UninstallPluginAsync(new StripePaymentElementsPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        var warnings = new List<string>();
        return await Task.FromResult(warnings);
    }

    public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        var request = new ProcessPaymentRequest();
        return Task.FromResult(request);
    }

    public Task<bool> CanRePostProcessPaymentAsync(Nop.Core.Domain.Orders.Order order)
    {
        return Task.FromResult(false);
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("NopStation.StripePaymentElement.PayNow", "Pay now"),
            new KeyValuePair<string, string>("NopStation.StripePaymentElement.PaymentError", "Stripe payment element error"),
            new KeyValuePair<string, string>("NopStation.StripePaymentElement.PaymentError.UnableToVerify", "Sorry, we were unable to verify your payment. Please contact with site maintainer."),

            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Menu.StripePaymentElement", "Stripe payment element"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Menu.Configuration", "Configuration"),

            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration", "Stripe payment element settings"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.AdditionalFee", "Additional fee"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.ApiKey", "Secret key"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.ApiKey.Hint", "Find the secret key from api keys tap of your stripe account dashboard."),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.TransactionMode", "Transaction mode"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.TransactionMode.Hint", "Select transaction mode."),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.Theme", "Theme"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.Theme.Hint", "Theme to use on payment page."),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.Layout", "Layout"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.Layout.Hint", "Layout to use on payment page."),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.EnableLogging", "Enable logging"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.EnableLogging.Hint", "Enable logging on stripe responses."),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.WebhookSecret", "Webhook secret key"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.WebhookSecret.Hint", "Webhook secret key"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.PublishableKey", "Publishable key"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.PublishableKey.Hint", "Find the publishable key from api keys tap of your stripe account dashboard."),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.AppleVerificationFileExist", "Apple verification file exist"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Fields.AppleVerificationFileExist.Hint", "Defines Apple verification file exist or not."),

            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Import.VerificationFile", "Verification File"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Import.AppleMarchentId", "Upload apple marchent verification file"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.Import.AppleVerfyFile", "Import apple verification file"),

            new KeyValuePair<string, string>("NopStation.StripePaymentElement.AppleVerification", "Apple verification upload status"),
            new KeyValuePair<string, string>("NopStation.StripePaymentElement.Configuration.PaymentMethodDescription", "Pay by stripe payment element"),

            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.AppleVerificationFileNotExist", "Please upload the apple verification file. Click the \"Import apple verification file\" button for upload and save."),

            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.UnsuccessfulUpload", "Verification file was not uploaded successfully"),
            new KeyValuePair<string, string>("Admin.NopStation.StripePaymentElement.Configuration.SuccessfulUpload", "Verification file was uploaded successfully")
        };

        return list;
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.StripePaymentElement.Menu.StripePaymentElement"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(StripePaymentElementsPermissionProvider.ManageConfiguration))
        {
            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.StripePaymentElement.Menu.Configuration"),
                Url = $"{_webHelper.GetStoreLocation()}Admin/StripePaymentElement/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "StripePaymentElement.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);
        }

        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/stripe-payment-element-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=stripe-payment-element",
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
        return await _localizationService.GetResourceAsync("NopStation.StripePaymentElement.Configuration.PaymentMethodDescription");
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.CheckoutPaymentInfoTop,
            PublicWidgetZones.OpcContentBefore
        });
    }

    public Type GetPublicViewComponent()
    {
        return typeof(StripePaymentElementViewComponent);
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone is null)
            throw new ArgumentNullException(nameof(widgetZone));

        if (widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) ||
            widgetZone.Equals(PublicWidgetZones.OpcContentBefore))
        {
            return typeof(StripePaymentElementScript);
        }

        return null;
    }

    #endregion
}
