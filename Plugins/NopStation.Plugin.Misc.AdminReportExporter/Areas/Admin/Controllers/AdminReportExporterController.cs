using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.AdminReportExporter.Areas.Admin.Model;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.AdminReportExporter.Areas.Admin.Controllers
{
    public class AdminReportExporterController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public AdminReportExporterController(ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreContext storeContext,
            ISettingService settingService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _settingService = settingService;
            _permissionService = permissionService;
        }

        #endregion

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AdminReportExporterSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                EnablePlugin = settings.EnablePlugin
            };
            if (storeScope > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnablePlugin, storeScope);
            }
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<AdminReportExporterSettings>(storeScope);

            settings.EnablePlugin = model.EnablePlugin;

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
