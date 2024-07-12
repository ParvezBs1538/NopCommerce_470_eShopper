using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ZohoSalesIQ.Models;

namespace NopStation.Plugin.Widgets.ZohoSalesIQ.Controllers
{
    public class ZohoSalesIQController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public ZohoSalesIQController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(ZohoSalesIQPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var zohoSalesIQSettings = await _settingService.LoadSettingAsync<ZohoSalesIQSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                EnablePlugin = zohoSalesIQSettings.EnablePlugin,
                Script = zohoSalesIQSettings.Script
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(zohoSalesIQSettings, x => x.EnablePlugin, storeScope);
                model.Script_OverrideForStore = await _settingService.SettingExistsAsync(zohoSalesIQSettings, x => x.Script, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Widgets.ZohoSalesIQ/Views/ZohoSalesIQ/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ZohoSalesIQPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var zohoSalesIQSettings = await _settingService.LoadSettingAsync<ZohoSalesIQSettings>(storeScope);

            //save settings
            zohoSalesIQSettings.EnablePlugin = model.EnablePlugin;
            zohoSalesIQSettings.Script = model.Script;

            await _settingService.SaveSettingOverridablePerStoreAsync(zohoSalesIQSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(zohoSalesIQSettings, x => x.Script, model.Script_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}