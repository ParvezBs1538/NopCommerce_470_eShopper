using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Http.Extensions;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.NetsEasy.Components;
using NopStation.Plugin.Payments.NetsEasy.Enums;
using NopStation.Plugin.Payments.NetsEasy.Models;
using NopStation.Plugin.Payments.NetsEasy.Models.Response;
using NopStation.Plugin.Payments.NetsEasy.Services;

namespace NopStation.Plugin.Payments.NetsEasy
{
    public class NetsEasyPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly INetsEasyPaymentService _netsEasyPaymentService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly NetsEasyPaymentSettings _netsEasyPaymentSettings;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public NetsEasyPaymentProcessor(IActionContextAccessor actionContextAccessor,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            INetsEasyPaymentService netsEasyPaymentService,
            WidgetSettings widgetSettings,
            ILocalizationService localizationService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ISettingService settingService,
            NetsEasyPaymentSettings netsEasyPaymentSettings,
            IWebHelper webHelper,
            ILogger logger,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            IOrderService orderService,
            IScheduleTaskService scheduleTaskService)
        {
            _actionContextAccessor = actionContextAccessor;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _netsEasyPaymentService = netsEasyPaymentService;
            _widgetSettings = widgetSettings;
            _localizationService = localizationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _settingService = settingService;
            _netsEasyPaymentSettings = netsEasyPaymentSettings;
            _webHelper = webHelper;
            _logger = logger;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _orderService = orderService;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Methods

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            //load settings for a chosen store scope
            var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id;
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeScope);

            //try to get an order id from custom values
            var paymentIdKey = await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.PaymentId");
            if (!processPaymentRequest.CustomValues.TryGetValue(paymentIdKey, out var paymentId) || string.IsNullOrEmpty(paymentId?.ToString()))
                throw new NopException("Failed to get the PayPal order ID");

            var orderTotal = processPaymentRequest.OrderTotal;

            var processPaymentResult = new ProcessPaymentResult();

            var payment = await _netsEasyPaymentService.RetrievePaymentAsync(paymentId.ToString(), storeScope);

            if (payment == null)
            {
                processPaymentResult.AddError(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.PaymentDoesNotExist"));
                return processPaymentResult;
            }

            if (!await VerifyReservedAmountAsync(payment, orderTotal))
            {
                processPaymentResult.AddError(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.PaidAmountMismatch"));

                var amountInCurrentStoreCurrency = Convert.ToInt32(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotal, await _workContext.GetWorkingCurrencyAsync()) * 100);
                await _logger.ErrorAsync(string.Format(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.PaidAmountMismatch.Log"),
                    processPaymentRequest.CustomerId, payment.Summary.ReservedAmount, amountInCurrentStoreCurrency));

                return processPaymentResult;
            }

            if (netsEasyPaymentSettings.TransactMode == TransactMode.AuthorizeAndCapture)
            {
                var chargePayment = await _netsEasyPaymentService.ChargePaymentAsync(new CapturePaymentRequest()
                {
                    Order = new Nop.Core.Domain.Orders.Order
                    {
                        AuthorizationTransactionId = payment.PaymentId,
                        OrderTotal = orderTotal,
                        CustomerCurrencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode
                    }
                }, storeScope);

                processPaymentResult.AuthorizationTransactionId = payment.PaymentId;
                processPaymentResult.CaptureTransactionId = chargePayment.ChargeId;
                processPaymentResult.CaptureTransactionResult = "Payment Complete";
                processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                processPaymentResult.AuthorizationTransactionId = payment.PaymentId;
                processPaymentResult.AuthorizationTransactionResult = "Authorization Complete";
                processPaymentResult.NewPaymentStatus = PaymentStatus.Authorized;
            }
            if (payment.Subscription != null && !string.IsNullOrEmpty(payment.Subscription.Id))
            {
                processPaymentResult.SubscriptionTransactionId = payment.Subscription.Id;
            }
            return processPaymentResult;
        }

        public async Task<bool> VerifyReservedAmountAsync(Payment payment, decimal amount)
        {
            var amountInCurrentStoreCurrency = Convert.ToInt32(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount, await _workContext.GetWorkingCurrencyAsync()) * 100);
            return payment.Summary.ReservedAmount == amountInCurrentStoreCurrency;
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            return Task.CompletedTask;
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are download able
            //or hide this payment method if current customer is from certain country
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _netsEasyPaymentSettings.AdditionalFee, _netsEasyPaymentSettings.AdditionalFeePercentage);
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id;

            var result = new CapturePaymentResult();

            var chargePaymentResult = await _netsEasyPaymentService.ChargePaymentAsync(capturePaymentRequest, storeScope);
            if (chargePaymentResult != null)
            {
                result.CaptureTransactionId = chargePaymentResult.ChargeId;
                result.CaptureTransactionResult = "Payment Complete";
                result.NewPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                result.NewPaymentStatus = capturePaymentRequest.Order.PaymentStatus;
                result.AddError("Failed to charge order amount");
            }

            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            //load settings for a chosen store scope
            var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id;

            var refundResult = await _netsEasyPaymentService.RefundChargeAsync(refundPaymentRequest, storeScope);

            if (refundResult == null)
            {
                var result = new RefundPaymentResult
                {
                    NewPaymentStatus = refundPaymentRequest.Order.PaymentStatus
                };

                return result;
            }

            //request succeeded

            await _genericAttributeService.SaveAttributeAsync(refundPaymentRequest.Order, "NetsEasyRefundId", refundResult.RefundId);

            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
            };
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            //load settings for a chosen store scope
            var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id;

            var cancelResult = await _netsEasyPaymentService.CancelPaymentAsync(voidPaymentRequest, storeScope);
            var voidResult = new VoidPaymentResult();

            if (!cancelResult)
            {
                voidResult.NewPaymentStatus = voidPaymentRequest.Order.PaymentStatus;
                voidResult.AddError("Could not void payment");
            }
            else
            {
                voidResult.NewPaymentStatus = PaymentStatus.Voided;
            }

            return voidResult;
        }

        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {

            var orderTotal = processPaymentRequest.OrderTotal;
            var processPaymentResult = new ProcessPaymentResult();

            if (processPaymentRequest.InitialOrder == null || string.IsNullOrEmpty(processPaymentRequest.InitialOrder.SubscriptionTransactionId))
            {
                return await ProcessPaymentAsync(processPaymentRequest);
            }
            var storeId = processPaymentRequest.InitialOrder.StoreId;
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeId);

            var paymentId = processPaymentRequest.InitialOrder.AuthorizationTransactionId;
            var payment = await _netsEasyPaymentService.RetrievePaymentAsync(paymentId.ToString(), storeId);
            var chargeSubscriptions = new ChargeSubscriptions();
            var subscription = new Models.Subscription();

            if (payment == null)
            {
                processPaymentResult.AddError(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.PaymentDoesNotExist"));
                return processPaymentResult;
            }
            var charge = payment.Charges.FirstOrDefault();

            if (payment.Charges == null)
            {
                processPaymentResult.AddError(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.ChargeNotFound"));
                return processPaymentResult;
            }

            subscription.SubscriptionId = processPaymentRequest.InitialOrder.SubscriptionTransactionId;
            subscription.Order = new Models.Order
            {
                Amount = payment.OrderDetails.Amount,
                Currency = payment.OrderDetails.Currency,
                Reference = payment.OrderDetails.Reference
            };

            foreach (var item in charge.OrderItems)
            {
                subscription.Order.Items.Add(new Item
                {
                    GrossTotalAmount = item.GrossTotalAmount,
                    Name = item.Name,
                    NetTotalAmount = item.NetTotalAmount,
                    Quantity = item.Quantity,
                    Reference = item.Reference,
                    TaxAmount = item.TaxAmount,
                    TaxRate = item.TaxRate,
                    Unit = item.Unit,
                    UnitPrice = item.UnitPrice
                });
            }
            chargeSubscriptions.Subscriptions.Add(subscription);
            chargeSubscriptions.ChargeId = processPaymentRequest.OrderGuid.ToString();

            var chargeSubscriptionsResponse = await _netsEasyPaymentService.ChargeSubscriptionAsync(chargeSubscriptions, storeId);
            if (chargeSubscriptionsResponse == null || string.IsNullOrEmpty(chargeSubscriptionsResponse.BulkId))
            {
                processPaymentResult.AddError(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.ChargeSubscriptionsResponseNull"));
                return processPaymentResult;
            }

            var retirveChargeSubscriptionsResponse = await _netsEasyPaymentService.RetrievesubscriptionChargeAsync(chargeSubscriptionsResponse.BulkId, storeId);
            if (retirveChargeSubscriptionsResponse == null)
            {
                processPaymentResult.AddError(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.RetirveChargeSubscriptionsResponseNull"));
                return processPaymentResult;
            }

            var retiveSubciption = retirveChargeSubscriptionsResponse.Pages.FirstOrDefault(x => x.SubscriptionId == processPaymentRequest.InitialOrder.SubscriptionTransactionId);
            if (retiveSubciption == null || string.IsNullOrEmpty(retiveSubciption.PaymentId))
            {
                processPaymentResult.AddError(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.RetiveSubciptionNull"));
                return processPaymentResult;
            }

            if (retiveSubciption.Status == NetsPaymentStatus.Failed.ToString())
            {
                processPaymentResult.AddError(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.RecurringPaymentFailed"));
                processPaymentResult.RecurringPaymentFailed = true;
            }
            else if (retiveSubciption.Status == NetsPaymentStatus.Succeeded.ToString() && string.IsNullOrEmpty(retiveSubciption.ChargeId))
            {
                processPaymentResult.AuthorizationTransactionId = retiveSubciption.PaymentId;
                processPaymentResult.CaptureTransactionId = retiveSubciption.ChargeId;
                processPaymentResult.CaptureTransactionResult = "Payment Complete";
                processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                Thread.Sleep(TimeSpan.FromSeconds(20));
                var verifyPayment = await _netsEasyPaymentService.VerifyPaymentAsync(retiveSubciption.PaymentId, storeId);
                if (verifyPayment != null && verifyPayment.Charges != null)
                {
                    var chargePayment = verifyPayment.Charges.FirstOrDefault();
                    if (chargePayment != null && !string.IsNullOrEmpty(chargePayment.ChargeId) && string.Equals(chargePayment.ChargeId, retiveSubciption.ChargeId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        processPaymentResult.AuthorizationTransactionId = retiveSubciption.PaymentId;
                        processPaymentResult.CaptureTransactionId = retiveSubciption.ChargeId;
                        processPaymentResult.CaptureTransactionResult = "Payment Complete";
                        processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
                    }
                }
            }

            if (netsEasyPaymentSettings.EnableLog)
            {
                await _logger.ErrorAsync($"Nets recurring pay response #order-guid: {processPaymentRequest.OrderGuid}", new Exception(JsonConvert.SerializeObject(retirveChargeSubscriptionsResponse)));
            }

            return processPaymentResult;

        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            //always success
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        public Task<bool> CanRePostProcessPaymentAsync(Nop.Core.Domain.Orders.Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return Task.FromResult(false);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.CheckoutPaymentInfoTop,
                PublicWidgetZones.OpcContentBefore,
                PublicWidgetZones.Footer
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));

            if (widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) ||
                widgetZone.Equals(PublicWidgetZones.OpcContentBefore) ||
                widgetZone.Equals(PublicWidgetZones.Footer))
            {
                return typeof(NetsEasyPaymentScriptsViewComponent);
            }
            return null;
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/NetsEasyPayment/Configure";
        }


        public override async Task InstallAsync()
        {
            //settings
            var settings = new NetsEasyPaymentSettings
            {
                TransactMode = TransactMode.Authorize,
                IntegrationType = IntegrationType.EmbeddedCheckout
            };
            await _settingService.SaveSettingAsync(settings);

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(NetsEasyPaymentDefaults.PluginSystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(NetsEasyPaymentDefaults.PluginSystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
            if (await _scheduleTaskService.GetTaskByTypeAsync(NetsEasyPaymentDefaults.SynchronizationTask) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = NetsEasyPaymentDefaults.DefaultSynchronizationPeriod * 60 * 60,
                    Name = NetsEasyPaymentDefaults.SynchronizationTaskName,
                    Type = NetsEasyPaymentDefaults.SynchronizationTask,
                    StopOnError = false,
                });
            }

            await this.InstallPluginAsync(new NetsEasyPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (await _scheduleTaskService.GetTaskByTypeAsync(NetsEasyPaymentDefaults.SynchronizationTask) is ScheduleTask task)
                await _scheduleTaskService.DeleteTaskAsync(task);

            //settings
            await _settingService.DeleteSettingAsync<NetsEasyPaymentSettings>();

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(NetsEasyPaymentDefaults.PluginSystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(NetsEasyPaymentDefaults.PluginSystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await this.UninstallPluginAsync(new NetsEasyPermissionProvider());
            await base.UninstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            if (await _scheduleTaskService.GetTaskByTypeAsync(NetsEasyPaymentDefaults.SynchronizationTask) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = NetsEasyPaymentDefaults.DefaultSynchronizationPeriod * 60 * 60,
                    Name = NetsEasyPaymentDefaults.SynchronizationTaskName,
                    Type = NetsEasyPaymentDefaults.SynchronizationTask,
                    StopOnError=false,
                });
            }
            var keyValuePairs = PluginResouces();
            foreach (var keyValuePair in keyValuePairs)
            {
                await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
            }

        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var errors = new List<string>();

            return Task.FromResult<IList<string>>(errors);
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            //already set
            return await Task.FromResult(await _actionContextAccessor.ActionContext.HttpContext.Session.GetAsync<ProcessPaymentRequest>(NetsEasyPaymentDefaults.PaymentRequestSessionKey));
        }

        public Type GetPublicViewComponent()
        {
            return typeof(NetsEasyPaymentInfoViewComponent);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            //locales
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.EnsureRecurringInterval", "Ensure Recurring Interval"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.EnsureRecurringInterval.Hint", "Enable this option to make sure that recurring payments are automatically charged at regular intervals"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.TransactMode", "Transaction Mode"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.TransactMode.Hint", "Specify transaction mode."),
                new KeyValuePair<string, string>("NopStation.NetsEasy.PaymentMethodDescription", "Pay by credit / debit card"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.TestMode", "Test Mode"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.TestMode.Hint", "Test Mode"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.CheckoutKey", "Checkout Key"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.CheckoutKey.Hint", "Checkout Key"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.SecretKey", "Secret Key"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.SecretKey.Hint", "Secret Key"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.CheckoutPageUrl", "Checkout Page Url"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.CheckoutPageUrl.Hint", "Checkout Page Url"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.ShowB2B", "Show B2B"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.ShowB2B.Hint", "Show B2B"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.EnableLog", "Enable log"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.EnableLog.Hint", "Enable log."),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.Countries", "Limit to Countries"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration.Fields.Countries.Hint", "Limit to Countries"),

                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.RecurringPaymentFailed", "Subscriptions charged Failed. Unable to Process next recurring order."),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.RetiveSubciptionNull", "Subscriptions charged Failed. Unable to Process next recurring order."),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.RetirveChargeSubscriptionsResponseNull", "Subscriptions charged Failed. Unable to Process next recurring order."),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.ChargeNotFound", "Subscription initail order not charged"),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.NotCharged", "Subscription initail order not charged"),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.ChargeSubscriptionsResponseNull", "Subscription initail order not charged"),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.SubscriptionDoesNotExist", "Subscription Does Not Exist"),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.PaymentDoesNotExist", "Payment Does Not Exist"),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.PaymentNotCompleted", "Payment Not Completed"),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.PaymentId", "NetsEasy Payment Id"),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.OpenPaymentWindow", "Open Payment Window"),

                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Menu.NetsEasy", "NetsEasy"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.NetsEasy.Configuration", "NetsEasy settings"),

                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.PaidAmountMismatch", "Your payment amount doesn't match order amount"),
                new KeyValuePair<string, string>("NopStation.NetsEasyPayment.PaidAmountMismatch.Log", "Nets Easy Payment Error: CustomerId: {0}, Paid Amount: {1}, Order Amount: {2}")
            };

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.NetsEasy.Menu.NetsEasy"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(NetsEasyPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.NetsEasy.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/NetsEasyPayment/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "NetsEasy.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/nets-easy-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=nets-easy",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        #endregion

        #region Properties

        public bool SupportCapture => true;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => true;

        public bool SupportVoid => true;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Manual;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.NetsEasy.PaymentMethodDescription");
        }

        public bool HideInWidgetList => false;

        #endregion
    }
}
