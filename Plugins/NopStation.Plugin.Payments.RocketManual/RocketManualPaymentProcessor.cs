using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.RocketManual.Components;

namespace NopStation.Plugin.Payments.RocketManual
{
    public class RocketManualPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly RocketManualPaymentSettings _rocketManualPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public RocketManualPaymentProcessor(RocketManualPaymentSettings rocketManualPaymentSettings,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _rocketManualPaymentSettings = rocketManualPaymentSettings;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
            return Task.CompletedTask;
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _rocketManualPaymentSettings.AdditionalFee, _rocketManualPaymentSettings.AdditionalFeePercentage);
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

            //it's not a redirection payment method. So we always return false
            return Task.FromResult(false);
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var result = new List<string>();
            if (form.TryGetValue("PhoneNumber", out var phone) && !string.IsNullOrWhiteSpace(phone.ToString()))
            {
                if (_rocketManualPaymentSettings.ValidatePhoneNumber)
                {
                    var regex = new Regex(_rocketManualPaymentSettings.PhoneNumberRegex);
                    if (!regex.Match(form["PhoneNumber"].ToString()).Success)
                        result.Add(await _localizationService.GetResourceAsync("NopStation.RocketManual.InvalidPhoneNumber"));
                }
            }
            else
            {
                result.Add(await _localizationService.GetResourceAsync("NopStation.RocketManual.EmptyPhoneNumber"));
            }

            if (!form.TryGetValue("TransactionId", out var tid) || string.IsNullOrWhiteSpace(tid.ToString()))
                result.Add(await _localizationService.GetResourceAsync("NopStation.RocketManual.EmptyTransactionId"));

            return result;
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var request = new ProcessPaymentRequest();
            request.CustomValues["Phone Number"] = form["PhoneNumber"].ToString();
            request.CustomValues["Transaction Id"] = form["TransactionId"].ToString();

            return Task.FromResult(request);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/RocketManual/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(RocketManualViewComponent);
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new RocketManualPaymentSettings
            {
                DescriptionText = "<p>Please enter Rocket phone number and transaction id. This will be checked manullay by admin.</p>",
                ValidatePhoneNumber = true,
                PhoneNumberRegex = "^(?:\\+88|88)?(01[3-9]\\d{8})$"
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new RocketManualPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<RocketManualPaymentSettings>();
            await this.UninstallPluginAsync(new RocketManualPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.RocketManual.PaymentMethodDescription");
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Menu.RocketManual", "Rocket (BD) manual"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Menu.Configuration", "Configuration"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration", "Rocket (BD) manual settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.AdditionalFee", "Additional fee"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.AdditionalFee.Hint", "The additional fee."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.DescriptionText", "Description"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.DescriptionText.Hint", "Enter info that will be shown to customers during checkout"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.RocketNumber", "Rocket number"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.RocketNumber.Hint", "Enter Rocket number."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.NumberType", "Number type"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.NumberType.Hint", "Select Rocket number type."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.ValidatePhoneNumber", "Validate phone number"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.ValidatePhoneNumber.Hint", "Determines whether to validate customer Rocket number in checkout."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.PhoneNumberRegex", "Phone number reg-ex"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.RocketManual.Configuration.Fields.PhoneNumberRegex.Hint", "Enter bangladeshi phone number requler expression (reg-ex)."));

            list.Add(new KeyValuePair<string, string>("NopStation.RocketManual.PaymentMethodDescription", "Pay by Rocket"));
            list.Add(new KeyValuePair<string, string>("NopStation.RocketManual.Fields.PhoneNumber", "Phone Number"));
            list.Add(new KeyValuePair<string, string>("NopStation.RocketManual.Fields.TransactionId", "Transaction Id"));
            list.Add(new KeyValuePair<string, string>("NopStation.RocketManual.InvalidPhoneNumber", "Invalid phone number."));
            list.Add(new KeyValuePair<string, string>("NopStation.RocketManual.EmptyPhoneNumber", "Please enter phone number."));
            list.Add(new KeyValuePair<string, string>("NopStation.RocketManual.EmptyTransactionId", "Please enter transaction id."));

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.RocketManual.Menu.RocketManual"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(RocketManualPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.RocketManual.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/RocketManual/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "RocketManual.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/Rocket-bd-manual-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=Rocket-bd-manual-payment",
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

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

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
    }
}