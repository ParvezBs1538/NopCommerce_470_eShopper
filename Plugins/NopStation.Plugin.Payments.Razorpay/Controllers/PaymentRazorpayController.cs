using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Razorpay.Models;

namespace NopStation.Plugin.Payments.Razorpay.Controllers
{
    public class PaymentRazorpayController : NopStationAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;

        public PaymentRazorpayController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(RazorpayPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var razorpayPaymentSettings = await _settingService.LoadSettingAsync<RazorpayPaymentSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                KeyId = razorpayPaymentSettings.KeyId,
                KeySecret = razorpayPaymentSettings.KeySecret,
                Description = razorpayPaymentSettings.Description,
                AdditionalFeePercentage = razorpayPaymentSettings.AdditionalFeePercentage,
                AdditionalFee = razorpayPaymentSettings.AdditionalFee
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.Description = await _localizationService
                    .GetLocalizedSettingAsync(razorpayPaymentSettings, x => x.Description, languageId, 0, false, false);
            });

            if (storeScope > 0)
            {
                model.Description_OverrideForStore = await _settingService.SettingExistsAsync(razorpayPaymentSettings, x => x.Description, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(razorpayPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(razorpayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.KeyId_OverrideForStore = await _settingService.SettingExistsAsync(razorpayPaymentSettings, x => x.KeyId, storeScope);
                model.KeySecret_OverrideForStore = await _settingService.SettingExistsAsync(razorpayPaymentSettings, x => x.KeySecret, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.Razorpay/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(RazorpayPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var razorpayPaymentSettings = await _settingService.LoadSettingAsync<RazorpayPaymentSettings>(storeScope);

            razorpayPaymentSettings.KeyId = model.KeyId;
            razorpayPaymentSettings.KeySecret = model.KeySecret;
            razorpayPaymentSettings.Description = model.Description;
            razorpayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            razorpayPaymentSettings.AdditionalFee = model.AdditionalFee;

            await _settingService.SaveSettingOverridablePerStoreAsync(razorpayPaymentSettings, x => x.KeyId, model.KeyId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(razorpayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(razorpayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(razorpayPaymentSettings, x => x.KeySecret, model.KeySecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(razorpayPaymentSettings, x => x.Description, model.Description_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(razorpayPaymentSettings,
                    x => x.Description, localized.LanguageId, localized.Description);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
