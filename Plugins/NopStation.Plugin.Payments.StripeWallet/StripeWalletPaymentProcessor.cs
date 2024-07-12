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
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.StripeWallet.Components;
using NopStation.Plugin.Payments.StripeWallet.Models;
using Stripe;

namespace NopStation.Plugin.Payments.StripeWallet
{
    public class StripeWalletPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly StripeWalletSettings _stripeWalletSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public StripeWalletPaymentProcessor(StripeWalletSettings stripeWalletSettings,
            ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService,
            IWorkContext workContext,
            IOrderService orderService,
            WidgetSettings widgetSettings)
        {
            _stripeWalletSettings = stripeWalletSettings;
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _logger = logger;
            _orderTotalCalculationService = orderTotalCalculationService;
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

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var processPaymentResult = new ProcessPaymentResult();

            if (processPaymentRequest.CustomValues.ContainsKey(StripeDefaults.PaymentIntentId))
                processPaymentResult.AuthorizationTransactionId = processPaymentRequest.CustomValues[StripeDefaults.PaymentIntentId].ToString();
            else
                processPaymentResult.Errors.Add("Payment intent id not found");

            return Task.FromResult(processPaymentResult);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var paymentIntentId = order.AuthorizationTransactionId;

            var service = new PaymentIntentService(new StripeClient(apiKey: _stripeWalletSettings.SecretKey));
            var paymentIntent = await service.GetAsync(paymentIntentId);

            if (paymentIntent == null || paymentIntent.Status != PaymentIntentStatus.requires_capture.ToString())
                return;

            if (_stripeWalletSettings.TransactionMode == TransactionMode.AuthorizeAndCapture)
            {
                var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _workContext.GetWorkingCurrencyAsync());
                var options = new PaymentIntentCaptureOptions
                {
                    AmountToCapture = (int)(orderTotal * 100),
                };
                var paymentIntentPaid = await service.CaptureAsync(paymentIntentId, options);

                if (paymentIntentPaid != null && paymentIntentPaid.Status == PaymentIntentStatus.succeeded.ToString())
                {
                    order.AuthorizationTransactionId = paymentIntentPaid.Id;
                    order.CaptureTransactionResult = paymentIntentPaid.Status;
                    order.CaptureTransactionId = paymentIntentPaid.Id;
                    order.PaymentStatus = PaymentStatus.Paid;

                    await _orderService.UpdateOrderAsync(order);
                }
            }
            else
            {
                order.AuthorizationTransactionResult = paymentIntent.Status;
                order.AuthorizationTransactionId = paymentIntentId;
                order.PaymentStatus = PaymentStatus.Authorized;

                await _orderService.UpdateOrderAsync(order);
            }
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
                _stripeWalletSettings.AdditionalFee, _stripeWalletSettings.AdditionalFeePercentage);
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            try
            {
                var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(capturePaymentRequest.Order.OrderTotal, await _workContext.GetWorkingCurrencyAsync());
                var options = new PaymentIntentCaptureOptions
                {
                    AmountToCapture = (int)(orderTotal * 100),
                };

                var service = new PaymentIntentService(new StripeClient(apiKey: _stripeWalletSettings.SecretKey));
                var paymentIntent = await service.CaptureAsync(capturePaymentRequest.Order.AuthorizationTransactionId, options);

                if (paymentIntent.Status == PaymentIntentStatus.succeeded.ToString())
                {
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    result.CaptureTransactionId = paymentIntent.Id;
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
                PaymentIntent = refundPaymentRequest.Order.CaptureTransactionId
            };

            var service = new RefundService(new StripeClient(apiKey: _stripeWalletSettings.SecretKey));
            var refund = service.Create(options);
            if (refund.Status == PaymentIntentStatus.succeeded.ToString())
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
            var service = new PaymentIntentService(new StripeClient(apiKey: _stripeWalletSettings.SecretKey));
            var paymentIntent = await service.CancelAsync(voidPaymentRequest.Order.AuthorizationTransactionId);

            if (paymentIntent != null && paymentIntent.Status == PaymentIntentStatus.canceled.ToString())
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

            switch (_stripeWalletSettings.TransactionMode)
            {
                case TransactionMode.Authorize:
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                    break;
                case TransactionMode.AuthorizeAndCapture:
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    break;
                default:
                    {
                        result.AddError("Not supported transaction type");
                        break;
                    }
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
            return $"{_webHelper.GetStoreLocation()}Admin/StripeWallet/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(StripeWalletViewComponent);
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new StripeWalletSettings
            {
                TransactionMode = TransactionMode.AuthorizeAndCapture
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new StripeWalletsPermissionProvider());

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
            await _settingService.DeleteSettingAsync<StripeWalletSettings>();
            await this.UninstallPluginAsync(new StripeWalletsPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();

            var model = new PaymentInfoModel
            {
                PaymentIntentId = form["PaymentIntentId"].ToString(),
                PaymentIntentStatus = form["PaymentIntentStatus"].ToString(),
            };

            if (string.IsNullOrWhiteSpace(model.PaymentIntentId) || string.IsNullOrWhiteSpace(model.PaymentIntentStatus))
                warnings.Add("Payment was not completed");
            else
            {
                var service = new PaymentIntentService(new StripeClient(apiKey: _stripeWalletSettings.SecretKey));
                var paymentIntent = await service.GetAsync(model.PaymentIntentId);

                if (paymentIntent.Status != PaymentIntentStatus.succeeded.ToString())
                {
                    if (_stripeWalletSettings.TransactionMode == TransactionMode.Authorize
                        && paymentIntent.Status != PaymentIntentStatus.requires_capture.ToString())
                    {
                        warnings.Add("Payment was not completed");
                    }
                }
            }

            return warnings;
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var request = new ProcessPaymentRequest();
            request.CustomValues[StripeDefaults.PaymentIntentId] = form["PaymentIntentId"].ToString();
            return Task.FromResult(request);
        }

        public Task<bool> CanRePostProcessPaymentAsync(Nop.Core.Domain.Orders.Order order)
        {
            return Task.FromResult(false);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Menu.StripeWallet", "Stripe wallet"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Menu.Configuration", "Configuration"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration", "Stripe digital wallet settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.AdditionalFee", "Additional fee"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.ApiKey", "Secret key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.ApiKey.Hint", "Find the secret key from api keys tap of your stripe account dashboard."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.TransactionMode", "Transaction mode"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.TransactionMode.Hint", "Select transaction mode."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.WebhookSecret", "Webhook secret key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.WebhookSecret.Hint", "Webhook secret key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.PublishableKey", "Publishable key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.PublishableKey.Hint", "Find the publishable key from api keys tap of your stripe account dashboard."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.AppleVerificationFileExist", "Apple verification file exist"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Fields.AppleVerificationFileExist.Hint", "Defines Apple verification file exist or not."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Import.VerificationFile", "Verification File"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Import.AppleMarchentId", "Upload apple marchent verification file"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.Import.AppleVerfyFile", "Import apple verification file"));

            list.Add(new KeyValuePair<string, string>("NopStation.StripeWallet.AppleVerification", "Apple verification upload status"));
            list.Add(new KeyValuePair<string, string>("NopStation.StripeWallet.Configuration.PaymentMethodDescription", "Pay by stripe digital wallets"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.AppleVerificationFileNotExist", "Please upload the apple verification file. Click the \"Import apple verification file\" button for upload and save."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.UnsuccessfulUpload", "Verification file was not uploaded successfully"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.StripeWallet.Configuration.SuccessfulUpload", "Verification file was uploaded successfully"));

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.StripeWallet.Menu.StripeWallet"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(StripeWalletsPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.StripeWallet.Menu.Configuration"),
                    Url = "~/Admin/StripeWallet/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "StripeWallet.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/stripe-digital-wallet-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=stripe-digital-wallet",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.StripeWallet.Configuration.PaymentMethodDescription");
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.CheckoutPaymentInfoTop,
                PublicWidgetZones.OpcContentBefore
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));

            if (widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) ||
                widgetZone.Equals(PublicWidgetZones.OpcContentBefore))
            {
                return typeof(StripeWalletScript);
            }
            return null;

        }


        #endregion
    }
}
