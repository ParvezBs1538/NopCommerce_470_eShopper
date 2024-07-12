using NopStation.Plugin.ExchangeRate.Abstract.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Filters;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.ExchangeRate.Abstract.Controllers
{
    public class AbstractExchangeRateController : NopStationAdminController
    {
        #region Fields

        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly AbstractExchangeRateSettings _abstractExchangeRateSettings;

        #endregion

        #region Ctor

        public AbstractExchangeRateController(INotificationService notificationService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            AbstractExchangeRateSettings abstractExchangeRateSettings)
        {
            _notificationService = notificationService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _abstractExchangeRateSettings = abstractExchangeRateSettings;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AbstractExchangeRatePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = new ConfigurationModel()
            {
                ApiKey = _abstractExchangeRateSettings.ApiKey
            };

            return View("~/Plugins/NopStation.Plugin.ExchangeRate.Abstract/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AbstractExchangeRatePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            _abstractExchangeRateSettings.ApiKey = model.ApiKey;
            await _settingService.SaveSettingAsync(_abstractExchangeRateSettings, x => x.ApiKey, 0, true);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
