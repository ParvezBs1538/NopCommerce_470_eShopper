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
using NopStation.Plugin.Widgets.Clarity.Models;

namespace NopStation.Plugin.Widgets.Clarity.Controllers
{
    public class ClarityController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public ClarityController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(ClarityPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var claritySettings = await _settingService.LoadSettingAsync<ClaritySettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                ProjectId = claritySettings.ProjectId,
                SettingModeId = (int)claritySettings.SettingMode,
                TrackingCode = claritySettings.TrackingCode,
                EnablePlugin = claritySettings.EnablePlugin
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(claritySettings, x => x.EnablePlugin, storeScope);
                model.ProjectId_OverrideForStore = await _settingService.SettingExistsAsync(claritySettings, x => x.ProjectId, storeScope);
                model.SettingModeId_OverrideForStore = await _settingService.SettingExistsAsync(claritySettings, x => x.SettingMode, storeScope);
                model.TrackingCode_OverrideForStore = await _settingService.SettingExistsAsync(claritySettings, x => x.TrackingCode, storeScope);
            }

            model.AvailableSettingModes = (await SettingMode.TrackingCode.ToSelectListAsync(false))
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            return View("~/Plugins/NopStation.Plugin.Widgets.Clarity/Views/Clarity/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ClarityPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var claritySettings = await _settingService.LoadSettingAsync<ClaritySettings>(storeScope);

            //save settings
            claritySettings.EnablePlugin = model.EnablePlugin;
            claritySettings.ProjectId = model.ProjectId;
            claritySettings.SettingMode = (SettingMode)model.SettingModeId;
            claritySettings.TrackingCode = model.TrackingCode;

            await _settingService.SaveSettingOverridablePerStoreAsync(claritySettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(claritySettings, x => x.ProjectId, model.ProjectId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(claritySettings, x => x.SettingMode, model.SettingModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(claritySettings, x => x.TrackingCode, model.TrackingCode_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}