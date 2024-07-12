using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.GoogleTagManager.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.GoogleTagManager.Areas.Admin.Controller
{
    public class GoogleTagManagerController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly INopFileProvider _fileProvider;

        #endregion

        #region Ctor

        public GoogleTagManagerController(IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            INopFileProvider fileProvider)
        {
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _fileProvider = fileProvider;
        }

        #endregion

        #region Methods

        #region Configuration
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(GoogleTagManagerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var googleTagManagerSettings = await _settingService.LoadSettingAsync<GoogleTagManagerSettings>(storeScope);

            var model = new ConfigurationModel
            {
                IsEnable = googleTagManagerSettings.IsEnable,
                GTMContainerId = googleTagManagerSettings.GTMContainerId,
            };
            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.IsEnable_OverrideForStore = await _settingService.SettingExistsAsync(googleTagManagerSettings, x => x.IsEnable, storeScope);
                model.GTMContainerId_OverrideForStore = await _settingService.SettingExistsAsync(googleTagManagerSettings, x => x.GTMContainerId, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Widgets.GoogleTagManager/Areas/Admin/Views/GoogleTagManager/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(GoogleTagManagerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var googleTagManagerSettings = await _settingService.LoadSettingAsync<GoogleTagManagerSettings>(storeScope);

            googleTagManagerSettings.IsEnable = model.IsEnable;
            googleTagManagerSettings.GTMContainerId = model.GTMContainerId;

            await _settingService.SaveSettingOverridablePerStoreAsync(googleTagManagerSettings, x => x.IsEnable, model.IsEnable_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(googleTagManagerSettings, x => x.GTMContainerId, model.GTMContainerId_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region ExportFile
        public async Task<IActionResult> ExportFile()
        {
            if (!await _permissionService.AuthorizeAsync(GoogleTagManagerPermissionProvider.ManageExportFile))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExportFile(FileInformationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(GoogleTagManagerPermissionProvider.ManageExportFile))
                return AccessDeniedView();

            try
            {
                var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/Plugins/NopStation.Plugin.Widgets.GoogleTagManager/"), "GTMPlugin.json");
                var fileInformation = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
                fileInformation = fileInformation.Replace("%GOOGLEANALYTICSID%", model.GAContainerId);
                return File(Encoding.UTF8.GetBytes(fileInformation), MimeTypes.ApplicationJson, "GTMPlugin.json");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ExportFile");
            }
        }

        #endregion

        #endregion
    }
}
