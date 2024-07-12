using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.SSLCommerz.Components;
using NopStation.Plugin.Payments.SSLCommerz.Sevices;

namespace NopStation.Plugin.Payments.SSLCommerz
{
    public class SSLCommerzPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly SSLCommerzPaymentSettings _sslcommerzPaymentSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ISSLCommerzManager _commerzManager;
        private readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public SSLCommerzPaymentProcessor(IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            SSLCommerzPaymentSettings sslcommerzPaymentSettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            ISSLCommerzManager commerzManager,
            IScheduleTaskService scheduleTaskService)
        {
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _sslcommerzPaymentSettings = sslcommerzPaymentSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _commerzManager = commerzManager;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var result = await _commerzManager.GetGatewayRedirectUrlAsync(order);

            if (result.Success)
                _httpContextAccessor.HttpContext.Response.Redirect(result.RedirectUrl);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _sslcommerzPaymentSettings.AdditionalFee, _sslcommerzPaymentSettings.AdditionalFeePercentage);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return await _commerzManager.RefundAsync(refundPaymentRequest);
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentSSLCommerz/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(SSLCommerzViewComponent);
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new SSLCommerzPaymentSettings
            {
                DescriptionText = "<p>You will be redirected to SSLCommerz site to complete the payment</p>"
            });

            if (await _scheduleTaskService.GetTaskByTypeAsync(Helper.ScheduleTaskType) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Name = "Update SSLCommerz refunds",
                    Seconds = 60,
                    Type = Helper.ScheduleTaskType
                });
            }

            await this.InstallPluginAsync(new SSLCommerzPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (await _scheduleTaskService.GetTaskByTypeAsync(Helper.ScheduleTaskType) is ScheduleTask scheduleTask)
                await _scheduleTaskService.DeleteTaskAsync(scheduleTask);

            await this.UninstallPluginAsync(new SSLCommerzPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>(new Dictionary<string, string>
            {
                ["Admin.NopStation.SSLCommerz.Configuration"] = "SSLCommerz settings",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.DescriptionText"] = "Description",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.DescriptionText.Hint"] = "Enter info that will be shown to customers during checkout",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.StoreID"] = "Store ID",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.StoreID.Hint"] = "Enter SSLCommerz store ID which is provided by SSLCommerz.",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.Password"] = "Password",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.Password.Hint"] = "Enter store password againstt this store ID.",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.AdditionalFee"] = "Additional fee",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.BusinessEmail"] = "Business email",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.BusinessEmail.Hint"] = "Enter SSLCommerz business email.",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.PassProductNamesAndTotals"] = "Pass product names and totals",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.PassProductNamesAndTotals.Hint"] = "Determines whether pass product names and order totals should be passed to SSLCommerz.",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.PDTToken"] = "PDT token",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.PDTToken.Hint"] = "Enter SSLCommerz PDT token which is provided by SSLCommerz.",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.UseSandbox"] = "Use sandbox",
                ["Admin.NopStation.SSLCommerz.Configuration.Fields.UseSandbox.Hint"] = "Check to enable testing environment (sandbox).",

                ["Admin.NopStation.SSLCommerz.Menu.SSLCommerz"] = "SSLCommerz",
                ["Admin.NopStation.SSLCommerz.Menu.Configuration"] = "Configuration",
                ["Admin.NopStation.SSLCommerz.RoundingWarning"] = "It looks like you have \"shoppingcartsettings.roundpricesduringcalculation\" <a href=\"{0}\" target=\"_blank\">setting</a> is disabled. Keep in mind that this can lead to a discrepancy of the order total amount, as SSLCommerz only rounds to two decimals.",
                ["NopStation.SSLCommerz.PaymentMethodDescription"] = "Pay by SSLCommerz",
            });

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SSLCommerz.Menu.SSLCommerz"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(SSLCommerzPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.SSLCommerz.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/PaymentSSLCommerz/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "SSLCommerz.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/sslcommerz-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=sslcommerz-payment",
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

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        public async Task<string> GetPaymentMethodDescriptionAsync() => await _localizationService.GetResourceAsync("NopStation.SSLCommerz.PaymentMethodDescription");

        #endregion
    }
}