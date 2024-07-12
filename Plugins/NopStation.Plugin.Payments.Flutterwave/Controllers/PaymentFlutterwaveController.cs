using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Flutterwave.Models;

namespace NopStation.Plugin.Payments.Flutterwave.Controllers
{
    public class PaymentFlutterwaveController : NopStationAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;

        public PaymentFlutterwaveController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(FlutterwavePaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var flutterwavePaymentSettings = await _settingService.LoadSettingAsync<FlutterwavePaymentSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                SecretKey = flutterwavePaymentSettings.SecretKey,
                PublicKey = flutterwavePaymentSettings.PublicKey,
                EncryptionKey = flutterwavePaymentSettings.EncryptionKey,
                Description = flutterwavePaymentSettings.Description,
                UseSandbox = flutterwavePaymentSettings.UseSandbox,
                AdditionalFeePercentage = flutterwavePaymentSettings.AdditionalFeePercentage,
                AdditionalFee = flutterwavePaymentSettings.AdditionalFee
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.Description = await _localizationService
                    .GetLocalizedSettingAsync(flutterwavePaymentSettings, x => x.Description, languageId, 0, false, false);
            });

            if (storeScope > 0)
            {
                model.Description_OverrideForStore = await _settingService.SettingExistsAsync(flutterwavePaymentSettings, x => x.Description, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(flutterwavePaymentSettings, x => x.UseSandbox, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(flutterwavePaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(flutterwavePaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(flutterwavePaymentSettings, x => x.SecretKey, storeScope);
                model.PublicKey_OverrideForStore = await _settingService.SettingExistsAsync(flutterwavePaymentSettings, x => x.PublicKey, storeScope);
                model.EncryptionKey_OverrideForStore = await _settingService.SettingExistsAsync(flutterwavePaymentSettings, x => x.EncryptionKey, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.Flutterwave/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FlutterwavePaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var flutterwavePaymentSettings = await _settingService.LoadSettingAsync<FlutterwavePaymentSettings>(storeScope);

            flutterwavePaymentSettings.SecretKey = model.SecretKey;
            flutterwavePaymentSettings.PublicKey = model.PublicKey;
            flutterwavePaymentSettings.EncryptionKey = model.EncryptionKey;
            flutterwavePaymentSettings.Description = model.Description;
            flutterwavePaymentSettings.UseSandbox = model.UseSandbox;
            flutterwavePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            flutterwavePaymentSettings.AdditionalFee = model.AdditionalFee;

            await _settingService.SaveSettingOverridablePerStoreAsync(flutterwavePaymentSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(flutterwavePaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(flutterwavePaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(flutterwavePaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(flutterwavePaymentSettings, x => x.PublicKey, model.PublicKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(flutterwavePaymentSettings, x => x.EncryptionKey, model.EncryptionKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(flutterwavePaymentSettings, x => x.Description, model.Description_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(flutterwavePaymentSettings,
                    x => x.Description, localized.LanguageId, localized.Description);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
