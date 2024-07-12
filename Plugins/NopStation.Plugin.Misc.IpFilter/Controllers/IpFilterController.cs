using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.IpFilter.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.IpFilter.Controllers
{
    public class IpFilterController : NopStationAdminController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IpFilterSettings _ipFilterSettings;

        #endregion

        #region Ctor

        public IpFilterController(ISettingService settingService, 
            IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IpFilterSettings ipFilterSettings)
        {
            _settingService = settingService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _ipFilterSettings = ipFilterSettings;
        }

        #endregion

        #region Configuration

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = new ConfigurationModel();
            model.IsEnabled = _ipFilterSettings.IsEnabled;
           
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            _ipFilterSettings.IsEnabled = model.IsEnabled;
            await _settingService.SaveSettingAsync(_ipFilterSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
