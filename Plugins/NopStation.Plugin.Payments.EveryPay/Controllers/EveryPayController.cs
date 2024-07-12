using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.EveryPay.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.Payments.EveryPay.Controllers
{
    public class EveryPayController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public EveryPayController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
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
            if (!await _permissionService.AuthorizeAsync(EveryPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var everyPaySettings = await _settingService.LoadSettingAsync<EveryPaySettings>(storeScope);

            var model = new ConfigurationModel
            {
                TransactModeId = everyPaySettings.TransactModeId,
                AdditionalFee = everyPaySettings.AdditionalFee,
                AdditionalFeePercentage = everyPaySettings.AdditionalFeePercentage,
                AvailableTransactModes = (await ((TransactMode)everyPaySettings.TransactModeId).ToSelectListAsync()).ToList(),
                ActiveStoreScopeConfiguration = storeScope,
                ApiKey = everyPaySettings.ApiKey,
                PrivateKey = everyPaySettings.PrivateKey,
                Installments = everyPaySettings.Installments ?? "",
                UseSandbox = everyPaySettings.UseSandbox
            };

            if (storeScope > 0)
            {
                model.TransactModeId_OverrideForStore = await _settingService.SettingExistsAsync(everyPaySettings, x => x.TransactModeId, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(everyPaySettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(everyPaySettings, x => x.AdditionalFeePercentage, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(everyPaySettings, x => x.ApiKey, storeScope);
                model.PrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(everyPaySettings, x => x.PrivateKey, storeScope);
                model.Installments_OverrideForStore = await _settingService.SettingExistsAsync(everyPaySettings, x => x.Installments, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(everyPaySettings, x => x.UseSandbox, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.EveryPay/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(EveryPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var everyPaySettings = await _settingService.LoadSettingAsync<EveryPaySettings>(storeScope);

            //save settings
            everyPaySettings.TransactModeId = model.TransactModeId;
            everyPaySettings.AdditionalFee = model.AdditionalFee;
            everyPaySettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            everyPaySettings.ApiKey = model.ApiKey;
            everyPaySettings.PrivateKey = model.PrivateKey;
            everyPaySettings.Installments = model.Installments ?? "";
            everyPaySettings.UseSandbox = model.UseSandbox;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            await _settingService.SaveSettingOverridablePerStoreAsync(everyPaySettings, x => x.TransactModeId, model.TransactModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(everyPaySettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(everyPaySettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(everyPaySettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(everyPaySettings, x => x.PrivateKey, model.PrivateKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(everyPaySettings, x => x.Installments, model.Installments_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(everyPaySettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}