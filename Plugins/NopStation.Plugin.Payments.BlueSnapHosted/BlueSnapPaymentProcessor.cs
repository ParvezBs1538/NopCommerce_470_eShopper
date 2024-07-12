using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.ScheduleTasks;
using NopStation.Plugin.Payments.BlueSnapHosted.Models;
using NopStation.Plugin.Payments.BlueSnapHosted.Services;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using Nop.Services.ScheduleTasks;
using Nop.Services.Orders;
using Nop.Services.Cms;
using Nop.Web.Framework.Infrastructure;
using Nop.Core.Domain.Cms;
using NopStation.Plugin.Payments.BlueSnapHosted.Components;

namespace NopStation.Plugin.Payments.BlueSnapHosted
{
    public class BlueSnapPaymentProcessor : BasePlugin, IPaymentMethod, IAdminMenuPlugin, INopStationPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly BlueSnapSettings _blueSnapSettings;
        private readonly IBlueSnapServices _blueSnapServices;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IStoreContext _storeContext;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public BlueSnapPaymentProcessor(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            BlueSnapSettings blueSnapSettings,
            IBlueSnapServices blueSnapServices,
            IScheduleTaskService scheduleTaskService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IWorkContext workContext,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            IOrderTotalCalculationService orderTotalCalculationService,
            WidgetSettings widgetSettings)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _blueSnapSettings = blueSnapSettings;
            _blueSnapServices = blueSnapServices;
            _scheduleTaskService = scheduleTaskService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _workContext = workContext;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = false
            };

            if (await _blueSnapServices.BlueSnapPaymentAPIAsync(processPaymentRequest) is PaymentResponse paymentResponse)
            {
                if (paymentResponse.TransactionId != null)
                {
                    result.AuthorizationTransactionId = paymentResponse.TransactionId;
                    result.CaptureTransactionResult = paymentResponse.ProcessingInfo.ProcessingStatus is null ? ""
                        : paymentResponse.ProcessingInfo.ProcessingStatus;
                    result.NewPaymentStatus = PaymentStatus.Paid;
                }
                else if (paymentResponse.Errors.Count != 0)
                {
                    result.Errors = paymentResponse.Errors.Select(error => 
                        error?.Code == "14042" ? "Card informations is not valid" : 
                        error.ErrorName).ToList();
                }
            }
            else
            {
                result.AddError("Process payment failed.");
            }

            return result;
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
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
                _blueSnapSettings.AdditionalFee, _blueSnapSettings.AdditionalFeePercentage);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            var refundResponse = await _blueSnapServices.BlueSnapRefundAPIAsync(refundPaymentRequest);

            if (refundResponse != null)
            {
                if (refundResponse.Errors.Count != 0)
                {
                    foreach (var error in refundResponse.Errors)
                        result.AddError(error.ErrorName);
                }
                else
                    result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
            }
            else
            {
                result.AddError("Failed");
            }

            return result;
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = false
            };

            var planResponse = await _blueSnapServices.CreatePlanAsync(processPaymentRequest);
            if (planResponse != null)
            {
                result.Cvv2Result = planResponse.PlanId.ToString();

                var subscriptionResponse = await _blueSnapServices.BlueSnapSubscriptionAPIAsync(planResponse.PlanId, processPaymentRequest);
                if (subscriptionResponse != null && subscriptionResponse.Status == "ACTIVE")
                {
                    result.SubscriptionTransactionId = subscriptionResponse.SubscriptionId.ToString();
                    result.AuthorizationTransactionId = subscriptionResponse.Charge.TransactionId;
                    result.NewPaymentStatus = PaymentStatus.Paid;
                }
                else
                    result.AddError("Subscription failed");
            }
            else
                result.AddError("Plan creation failed");

            return result;
        }

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var error = await _blueSnapServices.CancelRecurringPaymentAsync(cancelPaymentRequest);
            if (!string.IsNullOrWhiteSpace(error))
                return new CancelRecurringPaymentResult { Errors = new[] { error } };

            //always success
            return new CancelRecurringPaymentResult();
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return Task.FromResult(false);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var storeId = _storeContext.GetCurrentStore().Id;
            var requestForm = new ProcessPaymentRequest();

            if (form.ContainsKey("Token"))
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), BlueSnapDefaults.PfTokenAttribute, form["Token"].ToString(), storeId);

            if (form.ContainsKey("firstName"))
                requestForm.CustomValues.Add("FirstName", form["firstName"].ToString());
            if (form.ContainsKey("lastName"))
                requestForm.CustomValues.Add("LastName", form["lastName"].ToString());
            if (form.ContainsKey("Zip"))
                requestForm.CustomValues.Add("Zip", form["Zip"].ToString());

            return requestForm;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/BlueSnapHosted/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(BlueSnapHostedViewComponent);
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new BlueSnapSettings
            {
                IsSandBox = true
            };
            await _settingService.SaveSettingAsync(settings);

            //install synchronization task
            if (await _scheduleTaskService.GetTaskByTypeAsync(BlueSnapDefaults.SynchronizationTask) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = BlueSnapDefaults.DefaultSynchronizationPeriod * 60 * 60,
                    Name = BlueSnapDefaults.SynchronizationTaskName,
                    Type = BlueSnapDefaults.SynchronizationTask,
                });
            }

            await this.InstallPluginAsync(new BlueSnapPermissionProvider());

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(BlueSnapDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(BlueSnapDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<BlueSnapSettings>();

            //schedule task
            if (await _scheduleTaskService.GetTaskByTypeAsync(BlueSnapDefaults.SynchronizationTask) is ScheduleTask task)
                await _scheduleTaskService.DeleteTaskAsync(task);

            await this.UninstallPluginAsync(new BlueSnapPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>()
            {
                new("Admin.NopStation.BlueSnapHosted.Instructions", "This payment method stores credit card information in database (it's not sent to any third-party processor). In order to store credit card information, you must be PCI compliant."),
                
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.AdditionalFee", "Additional fee"),
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."),
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.Sandbox", "Use sandbox"),
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.Sandbox.Hint", "Use sandbox api if checkbox is enabled."),
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.Username", "Username"),
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.Username.Hint", "Specify BlueSnap username."),
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.Password", "Password"),
                new("Admin.NopStation.BlueSnapHosted.Configuration.Fields.Password.Hint", "Specify BlueSnap password."),
                new("Admin.NopStation.BlueSnapHosted.Configuration", "BlueSnap settings"),

                new("NopStation.BlueSnapHosted.PayNow", "Pay Now"),
                new("NopStation.BlueSnapHosted.Fields.FullName", "Card Holder Name"),
                new("NopStation.BlueSnapHosted.Fields.CardNumber", "Card Number"),
                new("NopStation.BlueSnapHosted.Fields.CardCode", "Card Code"),
                new("NopStation.BlueSnapHosted.Fields.ExpirationDate", "Exp. Date"),

                new("NopStation.BlueSnapHosted.PaymentMethodDescription", "Pay by credit / debit card"),

                new("Admin.NopStation.BlueSnapHosted.Menu.BlueSnapHosted", "BlueSnap (hosted)"),
                new("Admin.NopStation.BlueSnapHosted.Menu.Configuration", "Configuration")
            };

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.BlueSnapHosted.Menu.BlueSnapHosted"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(BlueSnapPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.BlueSnapHosted.Menu.Configuration"),
                    Url = "~/Admin/BlueSnapHosted/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "BlueSnapHosted"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/bluesnap-hosted-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=bluesnap-hosted",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
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
            return typeof(BlueSnapScriptViewComponent);
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Automatic;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        public bool HideInWidgetList => false;

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.BlueSnapHosted.PaymentMethodDescription");
        }

        #endregion
    }
}