using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.Smartsupp.Models;

namespace NopStation.Plugin.Widgets.Smartsupp.Controllers
{
    public class SmartsuppController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public SmartsuppController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(SmartsuppPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var smartsuppSettings = await _settingService.LoadSettingAsync<SmartsuppSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                EnablePlugin = smartsuppSettings.EnablePlugin,
                Key = smartsuppSettings.Key,
                SettingModeId = (int)smartsuppSettings.SettingMode,
                Script = smartsuppSettings.Script
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(smartsuppSettings, x => x.EnablePlugin, storeScope);
                model.Key_OverrideForStore = await _settingService.SettingExistsAsync(smartsuppSettings, x => x.Key, storeScope);
                model.SettingModeId_OverrideForStore = await _settingService.SettingExistsAsync(smartsuppSettings, x => x.SettingMode, storeScope);
                model.Script_OverrideForStore = await _settingService.SettingExistsAsync(smartsuppSettings, x => x.Script, storeScope);
            }

            model.AvailableSettingModes = (await SettingMode.Key.ToSelectListAsync(false))
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            return View("~/Plugins/NopStation.Plugin.Widgets.Smartsupp/Views/Smartsupp/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SmartsuppPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var smartsuppSettings = await _settingService.LoadSettingAsync<SmartsuppSettings>(storeScope);

            //save settings
            smartsuppSettings.EnablePlugin = model.EnablePlugin;
            smartsuppSettings.Key = model.Key;
            smartsuppSettings.SettingMode = (SettingMode)model.SettingModeId;
            smartsuppSettings.Script = model.Script;

            await _settingService.SaveSettingOverridablePerStoreAsync(smartsuppSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(smartsuppSettings, x => x.Key, model.Key_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(smartsuppSettings, x => x.SettingMode, model.SettingModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(smartsuppSettings, x => x.Script, model.Script_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}