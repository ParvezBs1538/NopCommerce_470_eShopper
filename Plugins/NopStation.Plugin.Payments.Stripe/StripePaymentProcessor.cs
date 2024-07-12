using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.Stripe.Components;
using NopStation.Plugin.Payments.Stripe.Models;
using NopStation.Plugin.Payments.Stripe.Validators;
using Stripe;
using Order = Nop.Core.Domain.Orders.Order;

namespace NopStation.Plugin.Payments.Stripe
{
    public class StripePaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly StripePaymentSettings _stripePaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly CurrencySettings _currencySettings;

        #endregion

        #region Ctor
        public StripePaymentProcessor(StripePaymentSettings stripePaymentSettings,
            ISettingService settingService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IWorkContext workContext,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            ICustomerService customerService,
            IOrderTotalCalculationService orderTotalCalculationService,
            CurrencySettings currencySettings)
        {
            _stripePaymentSettings = stripePaymentSettings;
            _settingService = settingService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _workContext = workContext;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _customerService = customerService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencySettings = currencySettings;
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

        #endregion

        #region Methods

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = false
            };

            var options = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Number = processPaymentRequest.CreditCardNumber,
                    ExpYear = long.Parse(processPaymentRequest.CreditCardExpireYear.ToString()),
                    ExpMonth = long.Parse(processPaymentRequest.CreditCardExpireMonth.ToString()),
                    Cvc = processPaymentRequest.CreditCardCvv2,
                    Name = processPaymentRequest.CustomerId.ToString()
                },
            };

            var service = new TokenService(new StripeClient(_stripePaymentSettings.ApiKey));
            var token = service.Create(options);

            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            if (customer == null)
                throw new NopException(_localizationService.GetResourceAsync("NopStation.Stripe.CustomerNotFound").Result);

            var customerCurrency = await _currencyService.GetCurrencyByIdAsync(customer.CurrencyId ?? _currencySettings.PrimaryStoreCurrencyId);

            var currency = customerCurrency != null && customerCurrency.Published ? customerCurrency : await _workContext.GetWorkingCurrencyAsync();
            if (currency == null)
                throw new NopException(_localizationService.GetResourceAsync("NopStation.Stripe.CurrencyNotLoaded").Result);

            var chargeOptions = new ChargeCreateOptions
            {
                Amount = (int)(processPaymentRequest.OrderTotal * 100),
                Currency = currency.CurrencyCode,
                Description = processPaymentRequest.OrderGuid.ToString(),
                Source = token.Id
            };

            chargeOptions.Capture = _stripePaymentSettings.TransactionMode != TransactionMode.Authorize;

            var chargeService = new ChargeService(new StripeClient(_stripePaymentSettings.ApiKey));

            var stripeCharge = chargeService.Create(chargeOptions);

            if (stripeCharge.Status == "succeeded")
            {
                if (stripeCharge.Captured && _stripePaymentSettings.TransactionMode == TransactionMode.AuthorizeAndCapture)
                {
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    result.CaptureTransactionId = stripeCharge.Id;
                    result.CaptureTransactionResult = stripeCharge.Status;
                }
                else
                {
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                    result.AuthorizationTransactionId = stripeCharge.Id;
                    result.AuthorizationTransactionResult = stripeCharge.Status;
                }
            }
            else if (stripeCharge.Status == "pending")
            {
                result.NewPaymentStatus = PaymentStatus.Pending;
                result.CaptureTransactionId = stripeCharge.Id;
                result.CaptureTransactionResult = stripeCharge.Status;
            }
            else
                result.AddError(_localizationService.GetResourceAsync("NopStation.Stripe.TransactionFailed").Result);

            return result;
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
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
               _stripePaymentSettings.AdditionalFee, _stripePaymentSettings.AdditionalFeePercentage);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            var captureOptions = new ChargeCaptureOptions()
            {
                Amount = (int)(capturePaymentRequest.Order.OrderTotal * 100)
            };

            var chargeService = new ChargeService(new StripeClient(_stripePaymentSettings.ApiKey));

            var stripeCharge = chargeService.Capture(capturePaymentRequest.Order.AuthorizationTransactionId, captureOptions);
            if (stripeCharge.Captured && stripeCharge.Status == "succeeded")
            {
                result.NewPaymentStatus = PaymentStatus.Paid;
                result.CaptureTransactionId = stripeCharge.Id;
            }
            else
                result.AddError(_localizationService.GetResourceAsync("NopStation.Stripe.CaptureFailed").Result);

            return Task.FromResult(result);
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            var refundPaymentAmount = refundPaymentRequest.IsPartialRefund ? refundPaymentRequest.AmountToRefund : refundPaymentRequest.Order.OrderTotal;
            var options = new RefundCreateOptions
            {
                Amount = (long)(refundPaymentAmount * 100),
                Reason = RefundReasons.RequestedByCustomer,
                Charge = refundPaymentRequest.Order.CaptureTransactionId
            };

            var service = new RefundService(new StripeClient(_stripePaymentSettings.ApiKey));
            var refund = service.Create(options);
            if (refund.Status == "succeeded")
            {
                result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
            }
            else
            {
                result.AddError(_localizationService.GetResourceAsync("NopStation.Stripe.RefundFailed").Result);
            }
            return Task.FromResult(result);
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            var options = new RefundCreateOptions
            {
                Amount = (long)(voidPaymentRequest.Order.OrderTotal * 100),
                Reason = RefundReasons.RequestedByCustomer,
                Charge = voidPaymentRequest.Order.AuthorizationTransactionId
            };

            var service = new RefundService(new StripeClient(_stripePaymentSettings.ApiKey));
            var refund = service.Create(options);

            if (refund.Status == "succeeded")
            {
                result.NewPaymentStatus = PaymentStatus.Voided;
            }
            else
                result.AddError(_localizationService.GetResourceAsync("NopStation.Stripe.VoidFailed").Result);

            return Task.FromResult(result);
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            switch (_stripePaymentSettings.TransactionMode)
            {
                case TransactionMode.Authorize:
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                    break;
                case TransactionMode.AuthorizeAndCapture:
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    break;
                default:
                    {
                        result.AddError(_localizationService.GetResourceAsync("NopStation.Stripe.NotSupportedTransaction").Result);
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentStripe/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(StripePaymentViewComponent);
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new StripePaymentSettings
            {
                TransactionMode = TransactionMode.AuthorizeAndCapture
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new StripePaymentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<StripePaymentSettings>();

            await this.UninstallPluginAsync(new StripePaymentPermissionProvider());
            await base.UninstallAsync();
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                foreach (var error in validationResult.Errors)
                    warnings.Add(error.ErrorMessage);

            return Task.FromResult<IList<string>>(warnings);
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest
            {
                CreditCardType = form["CreditCardType"],
                CreditCardName = form["CardholderName"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = int.Parse(form["ExpireMonth"]),
                CreditCardExpireYear = int.Parse(form["ExpireYear"]),
                CreditCardCvv2 = form["CardCode"]
            };
            return Task.FromResult(paymentInfo);
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return Task.FromResult(false);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration.Fields.ApiKey", "Secret key"),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration.Fields.ApiKey.Hint", "The secret key."),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration.Fields.TransactionMode", "Transaction mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration.Fields.TransactionMode.Hint", "Select transaction mode."),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration.Fields.PaymentMethodDescription", "Pay by credit / debit card"),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Menu.StripePayment", "Stripe payment"),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Stripe.Configuration", "Stripe payment settings"),

                new KeyValuePair<string, string>("NopStation.Stripe.CreditCardType", "Card Type"),
                new KeyValuePair<string, string>("NopStation.Stripe.CardholderName", "Cardholder Name"),
                new KeyValuePair<string, string>("NopStation.Stripe.CardNumber", "Card Number"),
                new KeyValuePair<string, string>("NopStation.Stripe.ExpirationDate", "Exp. Date"),
                new KeyValuePair<string, string>("NopStation.Stripe.CardCode", "Card Code"),

                new KeyValuePair<string, string>("NopStation.Stripe.CustomerNotFound", "Customer is not found"),
                new KeyValuePair<string, string>("NopStation.Stripe.NotSupportedTransaction", "Not supported transaction type"),
                new KeyValuePair<string, string>("NopStation.Stripe.VoidFailed", "Void request failed"),
                new KeyValuePair<string, string>("NopStation.Stripe.RefundFailed", "Refund request failed"),
                new KeyValuePair<string, string>("NopStation.Stripe.CaptureFailed", "Capture request failed"),
                new KeyValuePair<string, string>("NopStation.Stripe.CurrencyNotLoaded", "Currency cannot be loaded"),
                new KeyValuePair<string, string>("NopStation.Stripe.TransactionFailed", "Transaction failed"),
            };

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Stripe.Menu.StripePayment"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(StripePaymentPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Stripe.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/PaymentStripe/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "StripePayment.Configuration"
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
            return await _localizationService.GetResourceAsync("Admin.NopStation.Stripe.Configuration.Fields.PaymentMethodDescription");
        }

        #endregion
    }
}
