using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.PayHere.Models;

namespace NopStation.Plugin.Payments.PayHere.Controllers
{
    public class PaymentPayHereController : NopStationAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;

        public PaymentPayHereController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(PayHerePaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var payHerePaymentSettings = await _settingService.LoadSettingAsync<PayHerePaymentSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                MerchantId = payHerePaymentSettings.MerchantId,
                MerchantSecret = payHerePaymentSettings.MerchantSecret,
                Description = payHerePaymentSettings.Description,
                UseSandbox = payHerePaymentSettings.UseSandbox,
                AdditionalFeePercentage = payHerePaymentSettings.AdditionalFeePercentage,
                AdditionalFee = payHerePaymentSettings.AdditionalFee
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.Description = await _localizationService
                    .GetLocalizedSettingAsync(payHerePaymentSettings, x => x.Description, languageId, 0, false, false);
            });

            if (storeScope > 0)
            {
                model.Description_OverrideForStore = await _settingService.SettingExistsAsync(payHerePaymentSettings, x => x.Description, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(payHerePaymentSettings, x => x.UseSandbox, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(payHerePaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(payHerePaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(payHerePaymentSettings, x => x.MerchantId, storeScope);
                model.MerchantSecret_OverrideForStore = await _settingService.SettingExistsAsync(payHerePaymentSettings, x => x.MerchantSecret, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.PayHere/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PayHerePaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var payHerePaymentSettings = await _settingService.LoadSettingAsync<PayHerePaymentSettings>(storeScope);

            payHerePaymentSettings.MerchantId = model.MerchantId;
            payHerePaymentSettings.MerchantSecret = model.MerchantSecret;
            payHerePaymentSettings.Description = model.Description;
            payHerePaymentSettings.UseSandbox = model.UseSandbox;
            payHerePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            payHerePaymentSettings.AdditionalFee = model.AdditionalFee;

            await _settingService.SaveSettingOverridablePerStoreAsync(payHerePaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payHerePaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payHerePaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payHerePaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payHerePaymentSettings, x => x.MerchantSecret, model.MerchantSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payHerePaymentSettings, x => x.Description, model.Description_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(payHerePaymentSettings,
                    x => x.Description, localized.LanguageId, localized.Description);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
