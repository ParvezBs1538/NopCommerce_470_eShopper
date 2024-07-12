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
using NopStation.Plugin.Payments.NagadManual.Components;

namespace NopStation.Plugin.Payments.NagadManual
{
    public class NagadManualPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly NagadManualPaymentSettings _nagadManualPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public NagadManualPaymentProcessor(NagadManualPaymentSettings nagadManualPaymentSettings,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _nagadManualPaymentSettings = nagadManualPaymentSettings;
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
                _nagadManualPaymentSettings.AdditionalFee, _nagadManualPaymentSettings.AdditionalFeePercentage);
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
                if (_nagadManualPaymentSettings.ValidatePhoneNumber)
                {
                    var regex = new Regex(_nagadManualPaymentSettings.PhoneNumberRegex);
                    if (!regex.Match(form["PhoneNumber"].ToString()).Success)
                        result.Add(await _localizationService.GetResourceAsync("NopStation.NagadManual.InvalidPhoneNumber"));
                }
            }
            else
            {
                result.Add(await _localizationService.GetResourceAsync("NopStation.NagadManual.EmptyPhoneNumber"));
            }

            if (!form.TryGetValue("TransactionId", out var tid) || string.IsNullOrWhiteSpace(tid.ToString()))
                result.Add(await _localizationService.GetResourceAsync("NopStation.NagadManual.EmptyTransactionId"));

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
            return $"{_webHelper.GetStoreLocation()}Admin/NagadManual/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(NagadManualViewComponent);
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new NagadManualPaymentSettings
            {
                DescriptionText = "<p>Please enter Nagad phone number and transaction id. This will be checked manullay by admin.</p>",
                ValidatePhoneNumber = true,
                PhoneNumberRegex = "^(?:\\+88|88)?(01[3-9]\\d{8})$"
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new NagadManualPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<NagadManualPaymentSettings>();
            await this.UninstallPluginAsync(new NagadManualPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.NagadManual.PaymentMethodDescription");
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Menu.NagadManual", "Nagad manual"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Menu.Configuration", "Configuration"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration", "Nagad manual settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.AdditionalFee", "Additional fee"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.AdditionalFee.Hint", "The additional fee."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.DescriptionText", "Description"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.DescriptionText.Hint", "Enter info that will be shown to customers during checkout"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.NagadNumber", "Nagad number"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.NagadNumber.Hint", "Enter Nagad number."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.NumberType", "Number type"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.NumberType.Hint", "Select Nagad number type."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.ValidatePhoneNumber", "Validate phone number"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.ValidatePhoneNumber.Hint", "Determines whether to validate customer Nagad number in checkout."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.PhoneNumberRegex", "Phone number reg-ex"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.NagadManual.Configuration.Fields.PhoneNumberRegex.Hint", "Enter bangladeshi phone number requler expression (reg-ex)."));

            list.Add(new KeyValuePair<string, string>("NopStation.NagadManual.PaymentMethodDescription", "Pay by Nagad"));
            list.Add(new KeyValuePair<string, string>("NopStation.NagadManual.Fields.PhoneNumber", "Phone Number"));
            list.Add(new KeyValuePair<string, string>("NopStation.NagadManual.Fields.TransactionId", "Transaction Id"));
            list.Add(new KeyValuePair<string, string>("NopStation.NagadManual.InvalidPhoneNumber", "Invalid phone number."));
            list.Add(new KeyValuePair<string, string>("NopStation.NagadManual.EmptyPhoneNumber", "Please enter phone number."));
            list.Add(new KeyValuePair<string, string>("NopStation.NagadManual.EmptyTransactionId", "Please enter transaction id."));

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.NagadManual.Menu.NagadManual"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(NagadManualPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.NagadManual.Menu.Configuration"),
                    Url = "~/Admin/NagadManual/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "NagadManual.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/nagad-bd-manual-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=nagad-bd-manual-payment",
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