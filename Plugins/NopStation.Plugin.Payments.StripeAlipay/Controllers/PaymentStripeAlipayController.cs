using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.StripeAlipay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using System.Linq;
using Nop.Services.Directory;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.Payments.StripeAlipay.Controllers
{
    public class PaymentStripeAlipayController : NopStationAdminController
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

        public PaymentStripeAlipayController(ISettingService settingService,
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
            if (!await _permissionService.AuthorizeAsync(StripeAlipayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var alipayPaymentSettings = await _settingService.LoadSettingAsync<StripeAlipayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = alipayPaymentSettings.DescriptionText,
                AdditionalFee = alipayPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = alipayPaymentSettings.AdditionalFeePercentage,
                PublishableKey = alipayPaymentSettings.PublishableKey,
                ApiKey = alipayPaymentSettings.ApiKey,
                EnableWebhook = alipayPaymentSettings.EnableWebhook,
                WebhookSecret = alipayPaymentSettings.WebhookSecret,
                SupportedCurrencyCodes = alipayPaymentSettings.SupportedCurrencyCodes,
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
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(alipayPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(alipayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(alipayPaymentSettings, x => x.PublishableKey, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(alipayPaymentSettings, x => x.ApiKey, storeScope);
                model.EnableWebhook_OverrideForStore = await _settingService.SettingExistsAsync(alipayPaymentSettings, x => x.EnableWebhook, storeScope);
                model.WebhookSecret_OverrideForStore = await _settingService.SettingExistsAsync(alipayPaymentSettings, x => x.WebhookSecret, storeScope);
                model.SupportedCurrencyCodes_OverrideForStore = await _settingService.SettingExistsAsync(alipayPaymentSettings, x => x.SupportedCurrencyCodes, storeScope);
            }

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(alipayPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            return View("~/Plugins/NopStation.Plugin.Payments.StripeAlipay/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeAlipayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var alipayPaymentSettings = await _settingService.LoadSettingAsync<StripeAlipayPaymentSettings>(storeScope);

            //save settings
            alipayPaymentSettings.DescriptionText = model.DescriptionText;
            alipayPaymentSettings.AdditionalFee = model.AdditionalFee;
            alipayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            alipayPaymentSettings.ApiKey = model.ApiKey;
            alipayPaymentSettings.PublishableKey = model.PublishableKey;
            alipayPaymentSettings.EnableWebhook = model.EnableWebhook;
            alipayPaymentSettings.WebhookSecret = model.WebhookSecret;
            alipayPaymentSettings.SupportedCurrencyCodes = model.SupportedCurrencyCodes.ToList();

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(alipayPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(alipayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(alipayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(alipayPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(alipayPaymentSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(alipayPaymentSettings, x => x.EnableWebhook, model.EnableWebhook_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(alipayPaymentSettings, x => x.WebhookSecret, model.WebhookSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(alipayPaymentSettings, x => x.SupportedCurrencyCodes, model.SupportedCurrencyCodes_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(alipayPaymentSettings,
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
