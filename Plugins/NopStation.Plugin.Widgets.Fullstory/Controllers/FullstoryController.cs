using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.Fullstory.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace NopStation.Plugin.Widgets.Fullstory.Controllers
{
    public class FullstoryController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public FullstoryController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(FullstoryPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var fullstorySettings = await _settingService.LoadSettingAsync<FullstorySettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                OrganizationId = fullstorySettings.OrganizationId,
                SettingModeId = (int)fullstorySettings.SettingMode,
                Script = fullstorySettings.Script,
                EnablePlugin = fullstorySettings.EnablePlugin
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(fullstorySettings, x => x.EnablePlugin, storeScope);
                model.OrganizationId_OverrideForStore = await _settingService.SettingExistsAsync(fullstorySettings, x => x.OrganizationId, storeScope);
                model.SettingModeId_OverrideForStore = await _settingService.SettingExistsAsync(fullstorySettings, x => x.SettingMode, storeScope);
                model.Script_OverrideForStore = await _settingService.SettingExistsAsync(fullstorySettings, x => x.Script, storeScope);
            }

            model.AvailableSettingModes = (await SettingMode.Snippet.ToSelectListAsync(false))
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            return View("~/Plugins/NopStation.Plugin.Widgets.Fullstory/Views/Fullstory/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if(!await _permissionService.AuthorizeAsync(FullstoryPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var fullstorySettings = await _settingService.LoadSettingAsync<FullstorySettings>(storeScope);

            //save settings
            fullstorySettings.EnablePlugin = model.EnablePlugin;
            fullstorySettings.OrganizationId = model.OrganizationId;
            fullstorySettings.SettingMode = (SettingMode)model.SettingModeId;
            fullstorySettings.Script = model.Script;

            await _settingService.SaveSettingOverridablePerStoreAsync(fullstorySettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fullstorySettings, x => x.OrganizationId, model.OrganizationId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fullstorySettings, x => x.SettingMode, model.SettingModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fullstorySettings, x => x.Script, model.Script_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
           
            return RedirectToAction("Configure");
        }

        #endregion
    }
}