using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Instamojo.Models;

namespace NopStation.Plugin.Payments.Instamojo.Controllers
{
    public class PaymentInstamojoController : NopStationAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;

        public PaymentInstamojoController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPermissionService permissionService,
            ILanguageService languageService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _permissionService = permissionService;
            _languageService = languageService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(InstamojoPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var instamojoPaymentSettings = await _settingService.LoadSettingAsync<InstamojoPaymentSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                PrivateApiKey = instamojoPaymentSettings.PrivateApiKey,
                PrivateAuthToken = instamojoPaymentSettings.PrivateAuthToken,
                PrivateSalt = instamojoPaymentSettings.PrivateSalt,
                Description = instamojoPaymentSettings.Description,
                UseSandbox = instamojoPaymentSettings.UseSandbox,
                EnableSendEmail = instamojoPaymentSettings.EnableSendEmail,
                EnableSendSMS = instamojoPaymentSettings.EnableSendSMS,
                AdditionalFeePercentage = instamojoPaymentSettings.AdditionalFeePercentage,
                AdditionalFee = instamojoPaymentSettings.AdditionalFee,
                PhoneNumberRegex = instamojoPaymentSettings.PhoneNumberRegex
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.Description = await _localizationService
                    .GetLocalizedSettingAsync(instamojoPaymentSettings, x => x.Description, languageId, 0, false, false);
            });

            if (storeScope > 0)
            {
                model.Description_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.Description, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.UseSandbox, storeScope);
                model.EnableSendEmail_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.EnableSendEmail, storeScope);
                model.EnableSendSMS_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.EnableSendSMS, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PrivateApiKey_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.PrivateApiKey, storeScope);
                model.PrivateAuthToken_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.PrivateAuthToken, storeScope);
                model.PrivateSalt_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.PrivateSalt, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(instamojoPaymentSettings, x => x.PhoneNumberRegex, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.Instamojo/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(InstamojoPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var instamojoPaymentSettings = await _settingService.LoadSettingAsync<InstamojoPaymentSettings>(storeScope);

            instamojoPaymentSettings.PrivateApiKey = model.PrivateApiKey;
            instamojoPaymentSettings.PrivateAuthToken = model.PrivateAuthToken;
            instamojoPaymentSettings.PrivateSalt = model.PrivateSalt;
            instamojoPaymentSettings.Description = model.Description;
            instamojoPaymentSettings.UseSandbox = model.UseSandbox;
            instamojoPaymentSettings.EnableSendEmail = model.EnableSendEmail;
            instamojoPaymentSettings.EnableSendSMS = model.EnableSendSMS;
            instamojoPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            instamojoPaymentSettings.AdditionalFee = model.AdditionalFee;
            instamojoPaymentSettings.PhoneNumberRegex = model.PhoneNumberRegex;

            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.PrivateApiKey, model.PrivateApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.EnableSendSMS, model.EnableSendSMS_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.EnableSendEmail, model.EnableSendEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.PrivateAuthToken, model.PrivateAuthToken_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.PrivateSalt, model.PrivateSalt_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.Description, model.Description_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(instamojoPaymentSettings, x => x.PhoneNumberRegex, model.PhoneNumberRegex_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(instamojoPaymentSettings,
                    x => x.Description, localized.LanguageId, localized.Description);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
