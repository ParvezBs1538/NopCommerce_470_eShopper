using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.CopyAndPay.Models;

namespace NopStation.Plugin.Payments.CopyAndPay.Controllers
{
    public class CopyAndPayController : NopStationAdminController
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public CopyAndPayController(ILanguageService languageService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(CopyAndPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var cOPYandPayPaymentSettings = await _settingService.LoadSettingAsync<CopyAndPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                AdditionalFee = cOPYandPayPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = cOPYandPayPaymentSettings.AdditionalFeePercentage,
                EntityId = cOPYandPayPaymentSettings.EntityId,
                MadaEntityId = cOPYandPayPaymentSettings.MadaEntityId,
                TestMode = cOPYandPayPaymentSettings.TestMode,
                AuthorizationKey = cOPYandPayPaymentSettings.AuthorizationKey,
                APIUrl = cOPYandPayPaymentSettings.APIUrl,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(cOPYandPayPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(cOPYandPayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.EntityId_OverrideForStore = await _settingService.SettingExistsAsync(cOPYandPayPaymentSettings, x => x.EntityId, storeScope);
                model.MadaEntityId_OverrideForStore = await _settingService.SettingExistsAsync(cOPYandPayPaymentSettings, x => x.MadaEntityId, storeScope);
                model.TestMode_OverrideForStore = await _settingService.SettingExistsAsync(cOPYandPayPaymentSettings, x => x.TestMode, storeScope);
                model.AuthorizationKey_OverrideForStore = await _settingService.SettingExistsAsync(cOPYandPayPaymentSettings, x => x.AuthorizationKey, storeScope);
                model.APIUrl_OverrideForStore = await _settingService.SettingExistsAsync(cOPYandPayPaymentSettings, x => x.APIUrl, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.CopyAndPay/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(CopyAndPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var cOPYandPayPaymentSettings = await _settingService.LoadSettingAsync<CopyAndPayPaymentSettings>(storeScope);

            //save settings
            cOPYandPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            cOPYandPayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            cOPYandPayPaymentSettings.AuthorizationKey = model.AuthorizationKey;
            cOPYandPayPaymentSettings.EntityId = model.EntityId;
            cOPYandPayPaymentSettings.MadaEntityId = model.MadaEntityId;
            cOPYandPayPaymentSettings.TestMode = model.TestMode;
            cOPYandPayPaymentSettings.APIUrl = model.APIUrl;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(cOPYandPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cOPYandPayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cOPYandPayPaymentSettings, x => x.AuthorizationKey, model.AuthorizationKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cOPYandPayPaymentSettings, x => x.EntityId, model.EntityId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cOPYandPayPaymentSettings, x => x.MadaEntityId, model.MadaEntityId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cOPYandPayPaymentSettings, x => x.TestMode, model.TestMode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cOPYandPayPaymentSettings, x => x.APIUrl, model.APIUrl_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
