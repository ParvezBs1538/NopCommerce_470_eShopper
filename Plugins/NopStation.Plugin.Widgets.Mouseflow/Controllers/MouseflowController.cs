using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.Mouseflow.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace NopStation.Plugin.Widgets.Mouseflow.Controllers
{
    public class MouseflowController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public MouseflowController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(MouseflowPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mouseflowSettings = await _settingService.LoadSettingAsync<MouseflowSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                WebsiteId = mouseflowSettings.WebsiteId,
                SettingModeId = (int)mouseflowSettings.SettingMode,
                Script = mouseflowSettings.Script,
                EnablePlugin = mouseflowSettings.EnablePlugin
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(mouseflowSettings, x => x.EnablePlugin, storeScope);
                model.WebsiteId_OverrideForStore = await _settingService.SettingExistsAsync(mouseflowSettings, x => x.WebsiteId, storeScope);
                model.SettingModeId_OverrideForStore = await _settingService.SettingExistsAsync(mouseflowSettings, x => x.SettingMode, storeScope);
                model.Script_OverrideForStore = await _settingService.SettingExistsAsync(mouseflowSettings, x => x.Script, storeScope);
            }

            model.AvailableSettingModes = (await SettingMode.TrackingCode.ToSelectListAsync(false))
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            return View("~/Plugins/NopStation.Plugin.Widgets.Mouseflow/Views/Mouseflow/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if(!await _permissionService.AuthorizeAsync(MouseflowPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mouseflowSettings = await _settingService.LoadSettingAsync<MouseflowSettings>(storeScope);

            //save settings
            mouseflowSettings.EnablePlugin = model.EnablePlugin;
            mouseflowSettings.WebsiteId = model.WebsiteId;
            mouseflowSettings.SettingMode = (SettingMode)model.SettingModeId;
            mouseflowSettings.Script = model.Script;

            await _settingService.SaveSettingOverridablePerStoreAsync(mouseflowSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mouseflowSettings, x => x.WebsiteId, model.WebsiteId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mouseflowSettings, x => x.SettingMode, model.SettingModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mouseflowSettings, x => x.Script, model.Script_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
           
            return RedirectToAction("Configure");
        }

        #endregion
    }
}