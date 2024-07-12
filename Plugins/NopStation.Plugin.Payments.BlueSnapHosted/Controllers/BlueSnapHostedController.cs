using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Payments.BlueSnapHosted.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Controllers
{
    public class BlueSnapHostedController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public BlueSnapHostedController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var blueSnapSettings = await _settingService.LoadSettingAsync<BlueSnapSettings>(storeScope);

            var model = new ConfigurationModel
            {
                AdditionalFee = blueSnapSettings.AdditionalFee,
                IsSandBox = blueSnapSettings.IsSandBox,
                Username = blueSnapSettings.Username,
                Password = blueSnapSettings.Password,
                AdditionalFeePercentage = blueSnapSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(blueSnapSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(blueSnapSettings, x => x.AdditionalFeePercentage, storeScope);
                model.IsSandBox_OverrideForStore = await _settingService.SettingExistsAsync(blueSnapSettings, x => x.IsSandBox, storeScope);
                model.Username_OverrideForStore = await _settingService.SettingExistsAsync(blueSnapSettings, x => x.Username, storeScope);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(blueSnapSettings, x => x.Password, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.BlueSnapHosted/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var blueSnapSettings = await _settingService.LoadSettingAsync<BlueSnapSettings>(storeScope);

            //save settings
            blueSnapSettings.AdditionalFee = model.AdditionalFee;
            blueSnapSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            blueSnapSettings.IsSandBox = model.IsSandBox;
            blueSnapSettings.Username = model.Username;
            blueSnapSettings.Password = model.Password;

            await _settingService.SaveSettingOverridablePerStoreAsync(blueSnapSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blueSnapSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blueSnapSettings, x => x.IsSandBox, model.IsSandBox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blueSnapSettings, x => x.Username, model.Username_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blueSnapSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            
            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}