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
using NopStation.Plugin.Payments.StripeOxxo.Models;

namespace NopStation.Plugin.Payments.StripeOxxo.Controllers
{
    public class PaymentStripeOxxoController : NopStationAdminController
    {
        #region Field  

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ICurrencyService _currencyService;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public PaymentStripeOxxoController(ISettingService settingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService,
            ICurrencyService currencyService,
            ILanguageService languageService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _currencyService = currencyService;
            _languageService = languageService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StripeOxxoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var oxxoPaymentSettings = await _settingService.LoadSettingAsync<StripeOxxoPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = oxxoPaymentSettings.DescriptionText,
                AdditionalFee = oxxoPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = oxxoPaymentSettings.AdditionalFeePercentage,
                PublishableKey = oxxoPaymentSettings.PublishableKey,
                ApiKey = oxxoPaymentSettings.ApiKey,
                WebhookSecret = oxxoPaymentSettings.WebhookSecret,
                SupportedCurrencyCodes = oxxoPaymentSettings.SupportedCurrencyCodes,
                SendOrderInfoToStripe = oxxoPaymentSettings.SendOrderInfoToStripe,
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
                model.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(oxxoPaymentSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(oxxoPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(oxxoPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(oxxoPaymentSettings, x => x.PublishableKey, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(oxxoPaymentSettings, x => x.ApiKey, storeScope);
                model.WebhookSecret_OverrideForStore = await _settingService.SettingExistsAsync(oxxoPaymentSettings, x => x.WebhookSecret, storeScope);
                model.SupportedCurrencyCodes_OverrideForStore = await _settingService.SettingExistsAsync(oxxoPaymentSettings, x => x.SupportedCurrencyCodes, storeScope);
                model.SendOrderInfoToStripe_OverrideForStore = await _settingService.SettingExistsAsync(oxxoPaymentSettings, x => x.SendOrderInfoToStripe, storeScope);
            }

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(oxxoPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            return View("~/Plugins/NopStation.Plugin.Payments.StripeOxxo/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeOxxoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var oxxoPaymentSettings = await _settingService.LoadSettingAsync<StripeOxxoPaymentSettings>(storeScope);

            //save settings
            oxxoPaymentSettings.DescriptionText = model.DescriptionText;
            oxxoPaymentSettings.AdditionalFee = model.AdditionalFee;
            oxxoPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            oxxoPaymentSettings.ApiKey = model.ApiKey;
            oxxoPaymentSettings.PublishableKey = model.PublishableKey;
            oxxoPaymentSettings.WebhookSecret = model.WebhookSecret;
            oxxoPaymentSettings.SupportedCurrencyCodes = model.SupportedCurrencyCodes.ToList();
            oxxoPaymentSettings.SendOrderInfoToStripe = model.SendOrderInfoToStripe;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(oxxoPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oxxoPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oxxoPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oxxoPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oxxoPaymentSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oxxoPaymentSettings, x => x.WebhookSecret, model.WebhookSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oxxoPaymentSettings, x => x.SupportedCurrencyCodes, model.SupportedCurrencyCodes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oxxoPaymentSettings, x => x.SendOrderInfoToStripe, model.SendOrderInfoToStripe_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(oxxoPaymentSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
