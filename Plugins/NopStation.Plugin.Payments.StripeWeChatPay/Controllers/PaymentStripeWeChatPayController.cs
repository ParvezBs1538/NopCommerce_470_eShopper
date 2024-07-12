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
using NopStation.Plugin.Payments.StripeWeChatPay.Models;

namespace NopStation.Plugin.Payments.StripeWeChatPay.Controllers
{
    public class PaymentStripeWeChatPayController : NopStationAdminController
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

        public PaymentStripeWeChatPayController(ISettingService settingService,
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
            if (!await _permissionService.AuthorizeAsync(StripeWeChatPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var weChatPayPaymentSettings = await _settingService.LoadSettingAsync<StripeWeChatPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = weChatPayPaymentSettings.DescriptionText,
                AdditionalFee = weChatPayPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = weChatPayPaymentSettings.AdditionalFeePercentage,
                PublishableKey = weChatPayPaymentSettings.PublishableKey,
                ApiKey = weChatPayPaymentSettings.ApiKey,
                EnableWebhook = weChatPayPaymentSettings.EnableWebhook,
                WebhookSecret = weChatPayPaymentSettings.WebhookSecret,
                SupportedCurrencyCodes = weChatPayPaymentSettings.SupportedCurrencyCodes,
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
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(weChatPayPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(weChatPayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(weChatPayPaymentSettings, x => x.PublishableKey, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(weChatPayPaymentSettings, x => x.ApiKey, storeScope);
                model.EnableWebhook_OverrideForStore = await _settingService.SettingExistsAsync(weChatPayPaymentSettings, x => x.EnableWebhook, storeScope);
                model.WebhookSecret_OverrideForStore = await _settingService.SettingExistsAsync(weChatPayPaymentSettings, x => x.WebhookSecret, storeScope);
                model.SupportedCurrencyCodes_OverrideForStore = await _settingService.SettingExistsAsync(weChatPayPaymentSettings, x => x.SupportedCurrencyCodes, storeScope);
            }

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(weChatPayPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            return View("~/Plugins/NopStation.Plugin.Payments.StripeWeChatPay/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeWeChatPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var weChatPayPaymentSettings = await _settingService.LoadSettingAsync<StripeWeChatPayPaymentSettings>(storeScope);

            //save settings
            weChatPayPaymentSettings.DescriptionText = model.DescriptionText;
            weChatPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            weChatPayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            weChatPayPaymentSettings.ApiKey = model.ApiKey;
            weChatPayPaymentSettings.PublishableKey = model.PublishableKey;
            weChatPayPaymentSettings.EnableWebhook = model.EnableWebhook;
            weChatPayPaymentSettings.WebhookSecret = model.WebhookSecret;
            weChatPayPaymentSettings.SupportedCurrencyCodes = model.SupportedCurrencyCodes.ToList();

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(weChatPayPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(weChatPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(weChatPayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(weChatPayPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(weChatPayPaymentSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(weChatPayPaymentSettings, x => x.EnableWebhook, model.EnableWebhook_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(weChatPayPaymentSettings, x => x.WebhookSecret, model.WebhookSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(weChatPayPaymentSettings, x => x.SupportedCurrencyCodes, model.SupportedCurrencyCodes_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(weChatPayPaymentSettings,
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
