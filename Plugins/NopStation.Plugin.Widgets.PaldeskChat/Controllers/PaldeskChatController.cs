using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.PaldeskChat.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.PaldeskChat.Controllers
{
    public class PaldeskChatController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PaldeskChatController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(PaldeskChatPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paldeskChatSettings = await _settingService.LoadSettingAsync<PaldeskChatSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                EnablePlugin = paldeskChatSettings.EnablePlugin,
                Key = paldeskChatSettings.Key,
                SettingModeId = (int)paldeskChatSettings.SettingMode,
                Script = paldeskChatSettings.Script,
                ConfigureWithCustomerDataIfLoggedIn = paldeskChatSettings.ConfigureWithCustomerDataIfLoggedIn
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(paldeskChatSettings, x => x.EnablePlugin, storeScope);
                model.Key_OverrideForStore = await _settingService.SettingExistsAsync(paldeskChatSettings, x => x.Key, storeScope);
                model.SettingModeId_OverrideForStore = await _settingService.SettingExistsAsync(paldeskChatSettings, x => x.SettingMode, storeScope);
                model.Script_OverrideForStore = await _settingService.SettingExistsAsync(paldeskChatSettings, x => x.Script, storeScope);
                model.ConfigureWithCustomerDataIfLoggedIn_OverrideForStore = await _settingService.SettingExistsAsync(paldeskChatSettings, x => x.ConfigureWithCustomerDataIfLoggedIn, storeScope);
            }

            model.AvailableSettingModes = (await SettingMode.Key.ToSelectListAsync(false))
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            return View("~/Plugins/NopStation.Plugin.Widgets.PaldeskChat/Views/PaldeskChat/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if(!await _permissionService.AuthorizeAsync(PaldeskChatPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paldeskChatSettings = await _settingService.LoadSettingAsync<PaldeskChatSettings>(storeScope);

            //save settings
            paldeskChatSettings.EnablePlugin = model.EnablePlugin;
            paldeskChatSettings.Key = model.Key;
            paldeskChatSettings.SettingMode = (SettingMode)model.SettingModeId;
            paldeskChatSettings.Script = model.Script;
            paldeskChatSettings.ConfigureWithCustomerDataIfLoggedIn = model.ConfigureWithCustomerDataIfLoggedIn;

            await _settingService.SaveSettingOverridablePerStoreAsync(paldeskChatSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paldeskChatSettings, x => x.ConfigureWithCustomerDataIfLoggedIn, model.ConfigureWithCustomerDataIfLoggedIn_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paldeskChatSettings, x => x.Key, model.Key_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paldeskChatSettings, x => x.SettingMode, model.SettingModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paldeskChatSettings, x => x.Script, model.Script_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
           
            return RedirectToAction("Configure");
        }

        #endregion
    }
}