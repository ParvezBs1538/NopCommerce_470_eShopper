using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.Quickstream.Models;
using NopStation.Plugin.Payments.Quickstream.Services;
using NopStation.Plugin.Payments.Quickstream.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.ScheduleTasks;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Payments.Quickstream.Components;

namespace NopStation.Plugin.Payments.Quickstream
{
    public class QuickstreamPaymentProcessor : BasePlugin, IPaymentMethod, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IQuickStreamPaymentService _quickStreamPaymentService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public QuickstreamPaymentProcessor(ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IQuickStreamPaymentService quickStreamPaymentService,
            IScheduleTaskService scheduleTaskService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _quickStreamPaymentService = quickStreamPaymentService;
            _scheduleTaskService = scheduleTaskService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _logger = logger;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        #endregion

        #region Methods

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
            //if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
            return Task.FromResult(false);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            //TODO change later
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart, 0, false);
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/QuickStream/Configure";
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var token = await _quickStreamPaymentService.GetSingleUseTokenAsync(form);

            if (string.IsNullOrEmpty(token))
                return new ProcessPaymentRequest();

            return new ProcessPaymentRequest
            {
                CustomValues = new Dictionary<string, object>()
                {
                    ["Token"] = token,
                    ["Type"] = "CREDIT_CARD"
                }
            };
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.QuickStream.PaymentMethodDescription");
        }

        public Type GetPublicViewComponent()
        {
            return typeof(QuickStreamViewComponent);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public override async Task InstallAsync()
        {
            var task = await _scheduleTaskService.GetTaskByTypeAsync(QuickStreamDefaults.PAYMENT_STATUS_UPDATE);
            if (task == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "QuickStream Payment Status Update",
                    Seconds = 86400,
                    Type = QuickStreamDefaults.PAYMENT_STATUS_UPDATE,
                    StopOnError = false
                });
            }

            await this.InstallPluginAsync(new QuickstreamPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //deleting sceduleTask
            var task = await _scheduleTaskService.GetTaskByTypeAsync(QuickStreamDefaults.PAYMENT_STATUS_UPDATE);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);
            //settings
            await _settingService.DeleteSettingAsync<QuickstreamSettings>();

            await this.UninstallPluginAsync(new QuickstreamPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration", "QuickStream settings"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.UseSandbox.Hint", "Use sandbox."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.Username", "Username"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.Password", "Password"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.Username.Hint", "The username."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.Password.Hint", "The password."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.SupplierBusinessCode", "Supplier business code"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.SupplierBusinessCode.Hint", "The supplier business code."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.CommunityCode", "Community code"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.CommunityCode.Hint", "The community code."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.PublishableApiKey", "Publishable api key"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.PublishableApiKey.Hint", "The publishable api key."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.SecretApiKey", "Secret api key"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.SecretApiKey.Hint", "The secret api key."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.IpAddress", "Public ip address"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Configuration.Fields.IpAddress.Hint", "The public ip address."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Updated", "Accepted card details updated."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.List", "Accepted cards"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Sync", "Sync"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.EditDetails", "Edit accepted card details"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.BackToList", "back to accepted card list"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.CardScheme", "Card scheme"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.CardScheme.Hint", "The card scheme."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.CardType", "Card type"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.CardType.Hint", "The card type."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.PictureId", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.PictureId.Hint", "Card picture to show on checkout in accepted card list."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.Surcharge", "Surcharge %"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.Surcharge.Hint", "Change surcharge from quickstream portal and click sync on accepted card list page."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.IsEnable", "Enable"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.AcceptedCards.Fields.IsEnable.Hint", "To show in checkout accepted card list."),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Menu.QuickStream", "QuickStream payment"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.QuickStream.Menu.AcceptedCards", "Accepted cards"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentMethodDescription", "Pay by QuickStream credit card"),
                new KeyValuePair<string, string>("NopStation.QuickStream.CreditCardPayments.Fields.CardHolderName.Required", "Card holder name is required"),
                new KeyValuePair<string, string>("NopStation.QuickStream.CreditCardPayments.Fields.CardNumber.NotValid", "Card number is not valid"),
                new KeyValuePair<string, string>("NopStation.QuickStream.CreditCardPayments.Fields.CVN.NotValid", "CVN is not valid"),
                new KeyValuePair<string, string>("NopStation.QuickStream.CreditCardPayments.Fields.ExpireMonth.Required", "Expire Month is required"),
                new KeyValuePair<string, string>("NopStation.QuickStream.CreditCardPayments.Fields.ExpireYear.Required", "Expire Year is required"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.SelectPaymentType", "Transaction Type"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.CardholderName", "Cardholder Name"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.CardNumber", "Card Number"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.ExpirationDate", "Expiration Date"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.Cvn", "CVN"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.AccountName", "Account Name"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.Bsb", "BSB"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.AccountNumber", "Account Number"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.BankAccountName", "Bank Account Name"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.BankAccountNumber", "Bank Account Number"),
                new KeyValuePair<string, string>("NopStation.QuickStream.PaymentInfo.Fields.SurCharge", "Sur Charge"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.QuickStream.Menu.QuickStream"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(QuickstreamPermissionProvider.ManageConfiguration))
            {
                var configuration = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.QuickStream.Menu.Configuration"),
                    Url = "~/Admin/QuickStream/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "QuickStream.Configuration"
                };
                menuItem.ChildNodes.Add(configuration);

                var acceptCard = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.QuickStream.Menu.AcceptedCards"),
                    Url = "~/Admin/QuickStream/AcceptCardList",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "QuickStream.AcceptedCards"
                };
                menuItem.ChildNodes.Add(acceptCard);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/quickstream-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=quickstream-payment",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            return Task.CompletedTask;
        }

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var settings = await _settingService.LoadSettingAsync<QuickstreamSettings>();
            var result = new ProcessPaymentResult();

            try
            {
                if (processPaymentRequest.CustomValues.Keys.Contains("Token") && processPaymentRequest.CustomValues.Keys.Contains("Type"))
                {
                    var takePaymentResponse = await _quickStreamPaymentService.CompletePaymentAsync(processPaymentRequest);
                    if (takePaymentResponse != null && takePaymentResponse.SummaryCode == QuickStreamDefaults.APPROVED)
                    {
                        result.NewPaymentStatus = PaymentStatus.Paid;
                        result.CaptureTransactionId = takePaymentResponse.ReceiptNumber;
                        result.CaptureTransactionResult = takePaymentResponse.SummaryCode;
                        return result;
                    }
                    else if (takePaymentResponse != null && takePaymentResponse.SummaryCode == QuickStreamDefaults.NOT_APPROVED_YET)
                    {
                        result.NewPaymentStatus = PaymentStatus.Pending;
                        result.CaptureTransactionId = takePaymentResponse.ReceiptNumber;
                        result.CaptureTransactionResult = takePaymentResponse.SummaryCode;
                        return result;
                    }
                    else
                    {
                        result.AddError("Payment declined or rejected");
                    }
                }
                else
                {
                    result.AddError("Payment unsuccessful");
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                result.AddError("Payment is not completed");
                return result;
            }
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring method not supported" } });
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var refundResult = new RefundPaymentResult();
            var order = refundPaymentRequest.Order;
            var transaction = await _quickStreamPaymentService.GetTransactionByReceiptNumberAsync(refundPaymentRequest);
            if (!transaction.Refundable)
            {
                refundResult.AddError("Transaction is not refundable");
                return refundResult;
            }
            else if (refundPaymentRequest.IsPartialRefund && refundPaymentRequest.AmountToRefund >= order.OrderTotal)
            {
                refundResult.AddError("Partial refund can not be equal or greater than order total");
                return refundResult;
            }
            else
            {
                await _quickStreamPaymentService.RefundAsync(refundPaymentRequest);
                return new RefundPaymentResult
                {
                    NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
                };
            }
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();

            var creditCardvalidator = new CreditCardPaymentInfoValidator(_localizationService);
            var creditCardModel = new CreditCardPaymentModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };
            var creditCardvalidationResult = creditCardvalidator.Validate(creditCardModel);
            if (!creditCardvalidationResult.IsValid)
                warnings.AddRange(creditCardvalidationResult.Errors.Select(error => error.ErrorMessage));

            return Task.FromResult<IList<string>>(warnings);
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        #endregion
    }
}