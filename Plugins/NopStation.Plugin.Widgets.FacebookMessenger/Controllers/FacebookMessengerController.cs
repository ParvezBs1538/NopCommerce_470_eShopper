using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.FacebookMessenger.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;

namespace NopStation.Plugin.Widgets.FacebookMessenger.Controllers
{
    public class FacebookMessengerController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public FacebookMessengerController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(FacebookMessengerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var fbMessengerSettings = await _settingService.LoadSettingAsync<FacebookMessengerSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                PageId = fbMessengerSettings.PageId,
                ThemeColor = fbMessengerSettings.ThemeColor,
                EnableScript = fbMessengerSettings.EnableScript,
                Script = fbMessengerSettings.Script,
                EnablePlugin = fbMessengerSettings.EnablePlugin
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(fbMessengerSettings, x => x.EnablePlugin, storeScope);
                model.PageId_OverrideForStore = await _settingService.SettingExistsAsync(fbMessengerSettings, x => x.PageId, storeScope);
                model.ThemeColor_OverrideForStore = await _settingService.SettingExistsAsync(fbMessengerSettings, x => x.ThemeColor, storeScope);
                model.EnableScript_OverrideForStore = await _settingService.SettingExistsAsync(fbMessengerSettings, x => x.EnableScript, storeScope);
                model.Script_OverrideForStore = await _settingService.SettingExistsAsync(fbMessengerSettings, x => x.Script, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Widgets.FacebookMessenger/Views/FacebookMessenger/Configure.cshtml", model);
        }

        [EditAccess, HttpPost, FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if(!await _permissionService.AuthorizeAsync(FacebookMessengerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var fbMessengerSettings = await _settingService.LoadSettingAsync<FacebookMessengerSettings>(storeScope);

            //save settings
            fbMessengerSettings.PageId = model.PageId;
            fbMessengerSettings.ThemeColor = model.ThemeColor;
            fbMessengerSettings.EnableScript = model.EnableScript;
            fbMessengerSettings.Script = model.Script;
            fbMessengerSettings.EnablePlugin = model.EnablePlugin;

            await _settingService.SaveSettingOverridablePerStoreAsync(fbMessengerSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fbMessengerSettings, x => x.PageId, model.PageId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fbMessengerSettings, x => x.ThemeColor, model.ThemeColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fbMessengerSettings, x => x.EnableScript, model.EnableScript_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fbMessengerSettings, x => x.Script, model.Script_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
           
            return RedirectToAction("Configure");
        }

        #endregion
    }
}