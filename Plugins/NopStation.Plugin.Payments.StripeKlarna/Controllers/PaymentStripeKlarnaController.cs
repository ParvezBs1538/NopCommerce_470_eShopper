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
using Nop.Web.Areas.Admin.Factories;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.StripeKlarna.Models;

namespace NopStation.Plugin.Payments.StripeKlarna.Controllers
{
    public class PaymentStripeKlarnaController : NopStationAdminController
    {
        #region Field  

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public PaymentStripeKlarnaController(ISettingService settingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _languageService = languageService;
            _currencyService = currencyService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StripeKlarnaPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var klarnaPaymentSettings = await _settingService.LoadSettingAsync<StripeKlarnaPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = klarnaPaymentSettings.DescriptionText,
                AdditionalFee = klarnaPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = klarnaPaymentSettings.AdditionalFeePercentage,
                PublishableKey = klarnaPaymentSettings.PublishableKey,
                ApiKey = klarnaPaymentSettings.ApiKey,
                EnableWebhook = klarnaPaymentSettings.EnableWebhook,
                WebhookSecret = klarnaPaymentSettings.WebhookSecret,
                SupportedCurrencyCodes = klarnaPaymentSettings.SupportedCurrencyCodes,
                SupportedCountryIds = klarnaPaymentSettings.SupportedCountryIds,
                ActiveStoreScopeConfiguration = storeScope
            };

            var currencies = await _currencyService.GetAllCurrenciesAsync(showHidden: true);
            model.AvailableCurrencyCodes = currencies.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CurrencyCode
            }).ToList();

            await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries, false);

            if (storeScope > 0)
            {
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(klarnaPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(klarnaPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(klarnaPaymentSettings, x => x.PublishableKey, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(klarnaPaymentSettings, x => x.ApiKey, storeScope);
                model.EnableWebhook_OverrideForStore = await _settingService.SettingExistsAsync(klarnaPaymentSettings, x => x.EnableWebhook, storeScope);
                model.WebhookSecret_OverrideForStore = await _settingService.SettingExistsAsync(klarnaPaymentSettings, x => x.WebhookSecret, storeScope);
                model.SupportedCurrencyCodes_OverrideForStore = await _settingService.SettingExistsAsync(klarnaPaymentSettings, x => x.SupportedCurrencyCodes, storeScope);
                model.SupportedCountryIds_OverrideForStore = await _settingService.SettingExistsAsync(klarnaPaymentSettings, x => x.SupportedCountryIds, storeScope);
            }

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(klarnaPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            return View("~/Plugins/NopStation.Plugin.Payments.StripeKlarna/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeKlarnaPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var klarnaPaymentSettings = await _settingService.LoadSettingAsync<StripeKlarnaPaymentSettings>(storeScope);

            //save settings
            klarnaPaymentSettings.DescriptionText = model.DescriptionText;
            klarnaPaymentSettings.AdditionalFee = model.AdditionalFee;
            klarnaPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            klarnaPaymentSettings.ApiKey = model.ApiKey;
            klarnaPaymentSettings.PublishableKey = model.PublishableKey;
            klarnaPaymentSettings.EnableWebhook = model.EnableWebhook;
            klarnaPaymentSettings.WebhookSecret = model.WebhookSecret;
            klarnaPaymentSettings.SupportedCurrencyCodes = model.SupportedCurrencyCodes.ToList();
            klarnaPaymentSettings.SupportedCountryIds = model.SupportedCountryIds.ToList();

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(klarnaPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(klarnaPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(klarnaPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(klarnaPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(klarnaPaymentSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(klarnaPaymentSettings, x => x.EnableWebhook, model.EnableWebhook_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(klarnaPaymentSettings, x => x.WebhookSecret, model.WebhookSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(klarnaPaymentSettings, x => x.SupportedCurrencyCodes, model.SupportedCurrencyCodes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(klarnaPaymentSettings, x => x.SupportedCountryIds, model.SupportedCountryIds_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(klarnaPaymentSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
