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
using NopStation.Plugin.Payments.StripeSofort.Models;

namespace NopStation.Plugin.Payments.StripeSofort.Controllers
{
    public class PaymentStripeSofortController : NopStationAdminController
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

        public PaymentStripeSofortController(ISettingService settingService,
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
            if (!await _permissionService.AuthorizeAsync(StripeSofortPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var sofortPaymentSettings = await _settingService.LoadSettingAsync<StripeSofortPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = sofortPaymentSettings.DescriptionText,
                AdditionalFee = sofortPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = sofortPaymentSettings.AdditionalFeePercentage,
                PublishableKey = sofortPaymentSettings.PublishableKey,
                ApiKey = sofortPaymentSettings.ApiKey,
                WebhookSecret = sofortPaymentSettings.WebhookSecret,
                SupportedCurrencyCodes = sofortPaymentSettings.SupportedCurrencyCodes,
                SupportedCountryIds = sofortPaymentSettings.SupportedCountryIds,
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
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(sofortPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(sofortPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(sofortPaymentSettings, x => x.PublishableKey, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(sofortPaymentSettings, x => x.ApiKey, storeScope);
                model.WebhookSecret_OverrideForStore = await _settingService.SettingExistsAsync(sofortPaymentSettings, x => x.WebhookSecret, storeScope);
                model.SupportedCurrencyCodes_OverrideForStore = await _settingService.SettingExistsAsync(sofortPaymentSettings, x => x.SupportedCurrencyCodes, storeScope);
                model.SupportedCountryIds_OverrideForStore = await _settingService.SettingExistsAsync(sofortPaymentSettings, x => x.SupportedCountryIds, storeScope);
            }

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(sofortPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            return View("~/Plugins/NopStation.Plugin.Payments.StripeSofort/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeSofortPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var sofortPaymentSettings = await _settingService.LoadSettingAsync<StripeSofortPaymentSettings>(storeScope);

            //save settings
            sofortPaymentSettings.DescriptionText = model.DescriptionText;
            sofortPaymentSettings.AdditionalFee = model.AdditionalFee;
            sofortPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            sofortPaymentSettings.ApiKey = model.ApiKey;
            sofortPaymentSettings.PublishableKey = model.PublishableKey;
            sofortPaymentSettings.WebhookSecret = model.WebhookSecret;
            sofortPaymentSettings.SupportedCurrencyCodes = model.SupportedCurrencyCodes.ToList();
            sofortPaymentSettings.SupportedCountryIds = model.SupportedCountryIds.ToList();

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(sofortPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sofortPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sofortPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sofortPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sofortPaymentSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sofortPaymentSettings, x => x.WebhookSecret, model.WebhookSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sofortPaymentSettings, x => x.SupportedCurrencyCodes, model.SupportedCurrencyCodes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sofortPaymentSettings, x => x.SupportedCountryIds, model.SupportedCountryIds_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(sofortPaymentSettings,
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
