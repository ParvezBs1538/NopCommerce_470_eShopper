using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Controllers;
using NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Controllers
{
    public class DynamicSurveyController : BaseWidgetAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public DynamicSurveyController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            return RedirectToAction("Configure");
        }

        public virtual async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dynamicSurveySettings = await _settingService.LoadSettingAsync<DynamicSurveySettings>(storeScope);

            var model = dynamicSurveySettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.CaptchaEnabled_OverrideForStore = await _settingService.SettingExistsAsync(dynamicSurveySettings, x => x.CaptchaEnabled, storeScope);
                model.MinimumIntervalToSubmitSurvey_OverrideForStore = await _settingService.SettingExistsAsync(dynamicSurveySettings, x => x.MinimumIntervalToSubmitSurvey, storeScope);
                model.ImageSquarePictureSize_OverrideForStore = await _settingService.SettingExistsAsync(dynamicSurveySettings, x => x.ImageSquarePictureSize, storeScope);
                model.CountDisplayedYearsDatePicker_OverrideForStore = await _settingService.SettingExistsAsync(dynamicSurveySettings, x => x.CountDisplayedYearsDatePicker, storeScope);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dynamicSurveySettings = await _settingService.LoadSettingAsync<DynamicSurveySettings>(storeScope);
            dynamicSurveySettings = model.ToSettings(dynamicSurveySettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(dynamicSurveySettings, x => x.CaptchaEnabled, model.CaptchaEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dynamicSurveySettings, x => x.MinimumIntervalToSubmitSurvey, model.MinimumIntervalToSubmitSurvey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dynamicSurveySettings, x => x.ImageSquarePictureSize, model.ImageSquarePictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dynamicSurveySettings, x => x.CountDisplayedYearsDatePicker, model.CountDisplayedYearsDatePicker_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
