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
using NopStation.Plugin.Payments.StripeIdeal.Models;

namespace NopStation.Plugin.Payments.StripeIdeal.Controllers
{
    public class PaymentStripeIdealController : NopStationAdminController
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

        public PaymentStripeIdealController(ISettingService settingService,
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
            if (!await _permissionService.AuthorizeAsync(StripeIdealPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var idealPaymentSettings = await _settingService.LoadSettingAsync<StripeIdealPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = idealPaymentSettings.DescriptionText,
                AdditionalFee = idealPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = idealPaymentSettings.AdditionalFeePercentage,
                PublishableKey = idealPaymentSettings.PublishableKey,
                ApiKey = idealPaymentSettings.ApiKey,
                EnableWebhook = idealPaymentSettings.EnableWebhook,
                WebhookSecret = idealPaymentSettings.WebhookSecret,
                SupportedCurrencyCodes = idealPaymentSettings.SupportedCurrencyCodes,
                SupportedCountryIds = idealPaymentSettings.SupportedCountryIds,
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
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(idealPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(idealPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(idealPaymentSettings, x => x.PublishableKey, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(idealPaymentSettings, x => x.ApiKey, storeScope);
                model.EnableWebhook_OverrideForStore = await _settingService.SettingExistsAsync(idealPaymentSettings, x => x.EnableWebhook, storeScope);
                model.WebhookSecret_OverrideForStore = await _settingService.SettingExistsAsync(idealPaymentSettings, x => x.WebhookSecret, storeScope);
                model.SupportedCurrencyCodes_OverrideForStore = await _settingService.SettingExistsAsync(idealPaymentSettings, x => x.SupportedCurrencyCodes, storeScope);
                model.SupportedCountryIds_OverrideForStore = await _settingService.SettingExistsAsync(idealPaymentSettings, x => x.SupportedCountryIds, storeScope);
            }

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(idealPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            return View("~/Plugins/NopStation.Plugin.Payments.StripeIdeal/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeIdealPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var idealPaymentSettings = await _settingService.LoadSettingAsync<StripeIdealPaymentSettings>(storeScope);

            //save settings
            idealPaymentSettings.DescriptionText = model.DescriptionText;
            idealPaymentSettings.AdditionalFee = model.AdditionalFee;
            idealPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            idealPaymentSettings.ApiKey = model.ApiKey;
            idealPaymentSettings.PublishableKey = model.PublishableKey;
            idealPaymentSettings.EnableWebhook = model.EnableWebhook;
            idealPaymentSettings.WebhookSecret = model.WebhookSecret;
            idealPaymentSettings.SupportedCurrencyCodes = model.SupportedCurrencyCodes.ToList();
            idealPaymentSettings.SupportedCountryIds = model.SupportedCountryIds.ToList();

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(idealPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(idealPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(idealPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(idealPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(idealPaymentSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(idealPaymentSettings, x => x.EnableWebhook, model.EnableWebhook_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(idealPaymentSettings, x => x.WebhookSecret, model.WebhookSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(idealPaymentSettings, x => x.SupportedCurrencyCodes, model.SupportedCurrencyCodes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(idealPaymentSettings, x => x.SupportedCountryIds, model.SupportedCountryIds_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(idealPaymentSettings,
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
