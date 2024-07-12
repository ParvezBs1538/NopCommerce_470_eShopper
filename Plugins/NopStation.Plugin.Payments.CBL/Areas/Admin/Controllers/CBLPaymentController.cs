using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.CBL.Areas.Admin.Models;

namespace NopStation.Plugin.Payments.CBL.Areas.Admin.Controllers
{
    public class CBLPaymentController : NopStationAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        public CBLPaymentController(IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(CBLPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var cBLPaymentSettings = await _settingService.LoadSettingAsync<CBLPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                Debug = cBLPaymentSettings.Debug,
                UseSandbox = cBLPaymentSettings.UseSandbox,
                MerchantId = cBLPaymentSettings.MerchantId,
                MerchantUsername = cBLPaymentSettings.MerchantUsername,
                MerchantPassword = cBLPaymentSettings.MerchantPassword,
            };

            if (storeScope <= 0)
                return View(model);

            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(cBLPaymentSettings, x => x.UseSandbox, storeScope);
            model.Debug_OverrideForStore = await _settingService.SettingExistsAsync(cBLPaymentSettings, x => x.Debug, storeScope);
            model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(cBLPaymentSettings, x => x.MerchantId, storeScope);
            model.MerchantUsername_OverrideForStore = await _settingService.SettingExistsAsync(cBLPaymentSettings, x => x.MerchantUsername, storeScope);
            model.MerchantPassword_OverrideForStore = await _settingService.SettingExistsAsync(cBLPaymentSettings, x => x.MerchantPassword, storeScope);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(CBLPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            var storeScope = model.ActiveStoreScopeConfiguration;
            //load settings for a chosen store scope
            var cBLPaymentSettings = await _settingService.LoadSettingAsync<CBLPaymentSettings>(storeScope);

            cBLPaymentSettings.UseSandbox = model.UseSandbox;
            cBLPaymentSettings.Debug = model.Debug;
            cBLPaymentSettings.MerchantId = model.MerchantId;
            cBLPaymentSettings.MerchantUsername = model.MerchantUsername;
            cBLPaymentSettings.MerchantPassword = model.MerchantPassword;

            await _settingService.SaveSettingOverridablePerStoreAsync(cBLPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cBLPaymentSettings, x => x.Debug, model.Debug_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cBLPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cBLPaymentSettings, x => x.MerchantUsername, model.MerchantUsername_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cBLPaymentSettings, x => x.MerchantPassword, model.MerchantPassword_OverrideForStore, storeScope, false);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.CBL.Configuration.Saved"));

            return RedirectToAction("Configure");
        }
    }
}
