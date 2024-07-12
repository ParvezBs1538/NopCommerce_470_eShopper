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
using NopStation.Plugin.Payments.BkashManual.Components;

namespace NopStation.Plugin.Payments.BkashManual
{
    public class BkashManualPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly BkashManualSettings _bkashManualSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public BkashManualPaymentProcessor(BkashManualSettings bkashManualSettings,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _bkashManualSettings = bkashManualSettings;
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
                _bkashManualSettings.AdditionalFee, _bkashManualSettings.AdditionalFeePercentage);
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
                if (_bkashManualSettings.ValidatePhoneNumber)
                {
                    var regex = new Regex(_bkashManualSettings.PhoneNumberRegex);
                    if (!regex.Match(form["PhoneNumber"].ToString()).Success)
                        result.Add(await _localizationService.GetResourceAsync("NopStation.BkashManual.InvalidPhoneNumber"));
                }
            }
            else
            {
                result.Add(await _localizationService.GetResourceAsync("NopStation.BkashManual.EmptyPhoneNumber"));
            }

            if (!form.TryGetValue("TransactionId", out var tid) || string.IsNullOrWhiteSpace(tid.ToString()))
                result.Add(await _localizationService.GetResourceAsync("NopStation.BkashManual.EmptyTransactionId"));

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
            return $"{_webHelper.GetStoreLocation()}Admin/BkashManual/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(BkashManualViewComponent);
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new BkashManualSettings
            {
                DescriptionText = "<p>Please enter bKash phone number and transaction id. This will be checked manullay by admin.</p>",
                ValidatePhoneNumber = true,
                PhoneNumberRegex = "^(?:\\+88|88)?(01[3-9]\\d{8})$"
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new BkashManualPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<BkashManualSettings>();
            await this.UninstallPluginAsync(new BkashManualPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.BkashManual.PaymentMethodDescription");
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Menu.BkashManual", "bKash manual"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration", "bKash manual settings"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.AdditionalFee", "Additional fee"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.AdditionalFee.Hint", "The additional fee."),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.DescriptionText", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.DescriptionText.Hint", "Enter info that will be shown to customers during checkout"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.bKashNumber", "bKash number"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.bKashNumber.Hint", "Enter bKash number."),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.NumberType", "Number type"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.NumberType.Hint", "Select bKash number type."),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.ValidatePhoneNumber", "Validate phone number"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.ValidatePhoneNumber.Hint", "Determines whether to validate customer bKash number in checkout."),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.PhoneNumberRegex", "Phone number reg-ex"),
                new KeyValuePair<string, string>("Admin.NopStation.BkashManual.Configuration.Fields.PhoneNumberRegex.Hint", "Enter bangladeshi phone number requler expression (reg-ex)."),

                new KeyValuePair<string, string>("NopStation.BkashManual.PaymentMethodDescription", "Pay by bKash"),
                new KeyValuePair<string, string>("NopStation.BkashManual.Fields.PhoneNumber", "Phone Number"),
                new KeyValuePair<string, string>("NopStation.BkashManual.Fields.TransactionId", "Transaction Id"),
                new KeyValuePair<string, string>("NopStation.BkashManual.InvalidPhoneNumber", "Invalid phone number."),
                new KeyValuePair<string, string>("NopStation.BkashManual.EmptyPhoneNumber", "Please enter phone number."),
                new KeyValuePair<string, string>("NopStation.BkashManual.EmptyTransactionId", "Please enter transaction id.")
            };

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.BkashManual.Menu.BkashManual"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(BkashManualPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.BkashManual.Menu.Configuration"),
                    Url = "~/Admin/BkashManual/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "BkashManual.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/bkash-manual-payment-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=bkash-manual-payment",
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