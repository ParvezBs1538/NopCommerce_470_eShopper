using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.StripeGrabPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Directory;

namespace NopStation.Plugin.Payments.StripeGrabPay.Controllers
{
    public class PaymentStripeGrabPayController : NopStationAdminController
    {
        #region Field  

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public PaymentStripeGrabPayController(ISettingService settingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService,
            ILanguageService languageService,
            ICurrencyService currencyService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _languageService = languageService;
            _currencyService = currencyService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StripeGrabPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var grabPayPaymentSettings = await _settingService.LoadSettingAsync<StripeGrabPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = grabPayPaymentSettings.DescriptionText,
                AdditionalFee = grabPayPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = grabPayPaymentSettings.AdditionalFeePercentage,
                PublishableKey = grabPayPaymentSettings.PublishableKey,
                ApiKey = grabPayPaymentSettings.ApiKey,
                EnableWebhook = grabPayPaymentSettings.EnableWebhook,
                WebhookSecret = grabPayPaymentSettings.WebhookSecret,
                SupportedCurrencyCodes = grabPayPaymentSettings.SupportedCurrencyCodes,
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
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(grabPayPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(grabPayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(grabPayPaymentSettings, x => x.PublishableKey, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(grabPayPaymentSettings, x => x.ApiKey, storeScope);
                model.EnableWebhook_OverrideForStore = await _settingService.SettingExistsAsync(grabPayPaymentSettings, x => x.EnableWebhook, storeScope);
                model.WebhookSecret_OverrideForStore = await _settingService.SettingExistsAsync(grabPayPaymentSettings, x => x.WebhookSecret, storeScope);
                model.SupportedCurrencyCodes_OverrideForStore = await _settingService.SettingExistsAsync(grabPayPaymentSettings, x => x.SupportedCurrencyCodes, storeScope);
            }

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(grabPayPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            return View("~/Plugins/NopStation.Plugin.Payments.StripeGrabPay/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeGrabPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var grabPayPaymentSettings = await _settingService.LoadSettingAsync<StripeGrabPayPaymentSettings>(storeScope);

            //save settings
            grabPayPaymentSettings.DescriptionText = model.DescriptionText;
            grabPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            grabPayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            grabPayPaymentSettings.ApiKey = model.ApiKey;
            grabPayPaymentSettings.PublishableKey = model.PublishableKey;
            grabPayPaymentSettings.EnableWebhook = model.EnableWebhook;
            grabPayPaymentSettings.WebhookSecret = model.WebhookSecret;
            grabPayPaymentSettings.SupportedCurrencyCodes = model.SupportedCurrencyCodes.ToList();

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(grabPayPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(grabPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(grabPayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(grabPayPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(grabPayPaymentSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(grabPayPaymentSettings, x => x.EnableWebhook, model.EnableWebhook_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(grabPayPaymentSettings, x => x.WebhookSecret, model.WebhookSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(grabPayPaymentSettings, x => x.SupportedCurrencyCodes, model.SupportedCurrencyCodes_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(grabPayPaymentSettings,
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
