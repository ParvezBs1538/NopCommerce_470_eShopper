using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.CrispChat.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.CrispChat.Controllers
{
    public class CrispChatController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public CrispChatController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(CrispChatPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var crispChatSettings = await _settingService.LoadSettingAsync<CrispChatSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                EnablePlugin = crispChatSettings.EnablePlugin,
                WebsiteId = crispChatSettings.WebsiteId,
                SettingModeId = (int)crispChatSettings.SettingMode,
                Script = crispChatSettings.Script
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(crispChatSettings, x => x.EnablePlugin, storeScope);
                model.WebsiteId_OverrideForStore = await _settingService.SettingExistsAsync(crispChatSettings, x => x.WebsiteId, storeScope);
                model.SettingModeId_OverrideForStore = await _settingService.SettingExistsAsync(crispChatSettings, x => x.SettingMode, storeScope);
                model.Script_OverrideForStore = await _settingService.SettingExistsAsync(crispChatSettings, x => x.Script, storeScope);
            }

            model.AvailableSettingModes = (await SettingMode.WebsiteId.ToSelectListAsync(false))
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            return View("~/Plugins/NopStation.Plugin.Widgets.CrispChat/Views/CrispChat/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if(!await _permissionService.AuthorizeAsync(CrispChatPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var crispChatSettings = await _settingService.LoadSettingAsync<CrispChatSettings>(storeScope);

            //save settings
            crispChatSettings.EnablePlugin = model.EnablePlugin;
            crispChatSettings.WebsiteId = model.WebsiteId;
            crispChatSettings.SettingMode = (SettingMode)model.SettingModeId;
            crispChatSettings.Script = model.Script;

            await _settingService.SaveSettingOverridablePerStoreAsync(crispChatSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(crispChatSettings, x => x.WebsiteId, model.WebsiteId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(crispChatSettings, x => x.SettingMode, model.SettingModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(crispChatSettings, x => x.Script, model.Script_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
           
            return RedirectToAction("Configure");
        }

        #endregion
    }
}