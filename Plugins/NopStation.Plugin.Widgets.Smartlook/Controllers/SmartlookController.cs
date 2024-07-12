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
using Nop.Web.Areas.Admin.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.Smartlook.Models;

namespace NopStation.Plugin.Widgets.Smartlook.Controllers
{
    public class SmartlookController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public SmartlookController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(SmartlookPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var smartlookSettings = await _settingService.LoadSettingAsync<SmartlookSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                ProjectKey = smartlookSettings.ProjectKey,
                SettingModeId = (int)smartlookSettings.SettingMode,
                Script = smartlookSettings.Script,
                EnablePlugin = smartlookSettings.EnablePlugin
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(smartlookSettings, x => x.EnablePlugin, storeScope);
                model.ProjectKey_OverrideForStore = await _settingService.SettingExistsAsync(smartlookSettings, x => x.ProjectKey, storeScope);
                model.SettingModeId_OverrideForStore = await _settingService.SettingExistsAsync(smartlookSettings, x => x.SettingMode, storeScope);
                model.Script_OverrideForStore = await _settingService.SettingExistsAsync(smartlookSettings, x => x.Script, storeScope);
            }

            model.AvailableSettingModes = (await SettingMode.TrackingCode.ToSelectListAsync(false))
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            return View("~/Plugins/NopStation.Plugin.Widgets.Smartlook/Views/Smartlook/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SmartlookPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var smartlookSettings = await _settingService.LoadSettingAsync<SmartlookSettings>(storeScope);

            //save settings
            smartlookSettings.EnablePlugin = model.EnablePlugin;
            smartlookSettings.ProjectKey = model.ProjectKey;
            smartlookSettings.SettingMode = (SettingMode)model.SettingModeId;
            smartlookSettings.Script = model.Script;

            await _settingService.SaveSettingOverridablePerStoreAsync(smartlookSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(smartlookSettings, x => x.ProjectKey, model.ProjectKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(smartlookSettings, x => x.SettingMode, model.SettingModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(smartlookSettings, x => x.Script, model.Script_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}