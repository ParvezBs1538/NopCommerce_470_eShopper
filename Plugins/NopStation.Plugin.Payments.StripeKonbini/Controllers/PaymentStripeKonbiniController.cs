using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.StripeKonbini.Models;

namespace NopStation.Plugin.Payments.StripeKonbini.Controllers
{
    public class PaymentStripeKonbiniController : NopStationAdminController
    {
        #region Field  

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public PaymentStripeKonbiniController(ISettingService settingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService,
            ICurrencyService currencyService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _currencyService = currencyService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StripeKonbiniPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var konbiniPaymentSettings = await _settingService.LoadSettingAsync<StripeKonbiniPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                AdditionalFee = konbiniPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = konbiniPaymentSettings.AdditionalFeePercentage,
                PublishableKey = konbiniPaymentSettings.PublishableKey,
                ApiKey = konbiniPaymentSettings.ApiKey,
                WebhookSecret = konbiniPaymentSettings.WebhookSecret,
                SupportedCurrencyCodes = konbiniPaymentSettings.SupportedCurrencyCodes,
                SendOrderInfoToStripe = konbiniPaymentSettings.SendOrderInfoToStripe,
                ActiveStoreScopeConfiguration = storeScope
            };

            var currencies = await _currencyService.GetAllCurrenciesAsync(showHidden: true);
            model.AvailableCurrencyCodes = currencies.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CurrencyCode
            }).ToList();

            if (storeScope > 0)
            {
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(konbiniPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(konbiniPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(konbiniPaymentSettings, x => x.PublishableKey, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(konbiniPaymentSettings, x => x.ApiKey, storeScope);
                model.WebhookSecret_OverrideForStore = await _settingService.SettingExistsAsync(konbiniPaymentSettings, x => x.WebhookSecret, storeScope);
                model.SupportedCurrencyCodes_OverrideForStore = await _settingService.SettingExistsAsync(konbiniPaymentSettings, x => x.SupportedCurrencyCodes, storeScope);
                model.SendOrderInfoToStripe_OverrideForStore = await _settingService.SettingExistsAsync(konbiniPaymentSettings, x => x.SendOrderInfoToStripe, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.StripeKonbini/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeKonbiniPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var konbiniPaymentSettings = await _settingService.LoadSettingAsync<StripeKonbiniPaymentSettings>(storeScope);

            //save settings
            konbiniPaymentSettings.AdditionalFee = model.AdditionalFee;
            konbiniPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            konbiniPaymentSettings.ApiKey = model.ApiKey;
            konbiniPaymentSettings.PublishableKey = model.PublishableKey;
            konbiniPaymentSettings.WebhookSecret = model.WebhookSecret;
            konbiniPaymentSettings.SupportedCurrencyCodes = model.SupportedCurrencyCodes.ToList();
            konbiniPaymentSettings.SendOrderInfoToStripe = model.SendOrderInfoToStripe;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(konbiniPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(konbiniPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(konbiniPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(konbiniPaymentSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(konbiniPaymentSettings, x => x.WebhookSecret, model.WebhookSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(konbiniPaymentSettings, x => x.SupportedCurrencyCodes, model.SupportedCurrencyCodes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(konbiniPaymentSettings, x => x.SendOrderInfoToStripe, model.SendOrderInfoToStripe_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
