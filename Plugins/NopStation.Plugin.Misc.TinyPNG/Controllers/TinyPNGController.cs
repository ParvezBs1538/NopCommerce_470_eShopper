using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Messages;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.TinyPNG.Models;
using NopStation.Plugin.Misc.TinyPNG.Extensions;

namespace NopStation.Plugin.Misc.TinyPNG.Controllers
{
    public partial class TinyPNGController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public TinyPNGController(IStoreContext storeContext,
            IPermissionService permissionService,
            ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService)
        {
            _storeContext = storeContext;
            _permissionService = permissionService;
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(TinyPNGPermissionProvider.ManageConfiguration))
                return AccessDeniedView();


            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var tinyPNGSettings = await _settingService.LoadSettingAsync<TinyPNGSettings>(storeScope);
            var model = new ConfigurationModel
            {
                TinyPNGEnable = tinyPNGSettings.TinyPNGEnable,
                Keys = tinyPNGSettings.Keys,
                ApiUrl = tinyPNGSettings.ApiUrl,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.TinyPNGEnable_OverrideForStore = await _settingService.SettingExistsAsync(tinyPNGSettings, x => x.TinyPNGEnable, storeScope);
                model.ApiUrl_OverrideForStore = await _settingService.SettingExistsAsync(tinyPNGSettings, x => x.ApiUrl, storeScope);
                model.Keys_OverrideForStore = await _settingService.SettingExistsAsync(tinyPNGSettings, x => x.Keys, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Misc.TinyPNG/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(TinyPNGPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var tinyPNGSettings = await _settingService.LoadSettingAsync<TinyPNGSettings>(storeScope);

            //get previous picture identifiers
            tinyPNGSettings.TinyPNGEnable = model.TinyPNGEnable;
            tinyPNGSettings.Keys = model.Keys;
            tinyPNGSettings.ApiUrl = model.ApiUrl;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(tinyPNGSettings, x => x.TinyPNGEnable, model.TinyPNGEnable_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(tinyPNGSettings, x => x.ApiUrl, model.ApiUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(tinyPNGSettings, x => x.Keys, model.Keys_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
            return Redirect("Configure");
        }

        #endregion
    }
}
