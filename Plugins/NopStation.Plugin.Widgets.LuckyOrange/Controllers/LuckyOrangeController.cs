using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.LuckyOrange.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace NopStation.Plugin.Widgets.LuckyOrange.Controllers
{
    public class LuckyOrangeController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public LuckyOrangeController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(LuckyOrangePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var luckyOrangeSettings = await _settingService.LoadSettingAsync<LuckyOrangeSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                SiteId = luckyOrangeSettings.SiteId,
                SettingModeId = (int)luckyOrangeSettings.SettingMode,
                TrackingCode = luckyOrangeSettings.TrackingCode,
                EnablePlugin = luckyOrangeSettings.EnablePlugin
            };

            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(luckyOrangeSettings, x => x.EnablePlugin, storeScope);
                model.SiteId_OverrideForStore = await _settingService.SettingExistsAsync(luckyOrangeSettings, x => x.SiteId, storeScope);
                model.SettingModeId_OverrideForStore = await _settingService.SettingExistsAsync(luckyOrangeSettings, x => x.SettingMode, storeScope);
                model.TrackingCode_OverrideForStore = await _settingService.SettingExistsAsync(luckyOrangeSettings, x => x.TrackingCode, storeScope);
            }

            model.AvailableSettingModes = (await SettingMode.TrackingCode.ToSelectListAsync(false))
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            return View("~/Plugins/NopStation.Plugin.Widgets.LuckyOrange/Views/LuckyOrange/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if(!await _permissionService.AuthorizeAsync(LuckyOrangePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var luckyOrangeSettings = await _settingService.LoadSettingAsync<LuckyOrangeSettings>(storeScope);

            //save settings
            luckyOrangeSettings.EnablePlugin = model.EnablePlugin;
            luckyOrangeSettings.SiteId = model.SiteId;
            luckyOrangeSettings.SettingMode = (SettingMode)model.SettingModeId;
            luckyOrangeSettings.TrackingCode = model.TrackingCode;

            await _settingService.SaveSettingOverridablePerStoreAsync(luckyOrangeSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(luckyOrangeSettings, x => x.SiteId, model.SiteId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(luckyOrangeSettings, x => x.SettingMode, model.SettingModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(luckyOrangeSettings, x => x.TrackingCode, model.TrackingCode_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
           
            return RedirectToAction("Configure");
        }

        #endregion
    }
}