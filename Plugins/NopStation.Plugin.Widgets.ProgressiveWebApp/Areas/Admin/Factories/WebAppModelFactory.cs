using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public class WebAppModelFactory : IWebAppModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;

        #endregion

        #region Ctor

        public WebAppModelFactory(IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPushNotificationTemplateService pushNotificationTemplateService)
        {
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _pushNotificationTemplateService = pushNotificationTemplateService;
        }

        #endregion

        #region Methods

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var webAppSettings = await _settingService.LoadSettingAsync<ProgressiveWebAppSettings>(storeScope);

            var model = webAppSettings.ToSettingsModel<ConfigurationModel>();

            if (string.IsNullOrEmpty(model.ManifestThemeColor))
                model.ManifestThemeColor = "#000000";
            if (string.IsNullOrEmpty(model.ManifestBackgroundColor))
                model.ManifestBackgroundColor = "#000000";

            model.AvailableDisplayTypes.Add(new SelectListItem() { Value = "browser", Text = "Browser" });
            model.AvailableDisplayTypes.Add(new SelectListItem() { Value = "standalone", Text = "Standalone" });
            model.AvailableDisplayTypes.Add(new SelectListItem() { Value = "minimal-ui", Text = "Minimal UI" });
            model.AvailableDisplayTypes.Add(new SelectListItem() { Value = "fullscreen", Text = "Fullscreen" });

            model.ActiveStoreScopeConfiguration = storeScope;

            model.AvailableUnits = (await UnitType.Days.ToSelectListAsync()).ToList();

            if (storeScope > 0)
            {
                model.DefaultIconId_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.DefaultIconId, storeScope);
                model.DisableSilent_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.DisableSilent, storeScope);
                model.ManifestAppScope_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.ManifestAppScope, storeScope);
                model.ManifestBackgroundColor_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.ManifestBackgroundColor, storeScope);
                model.ManifestDisplay_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.ManifestDisplay, storeScope);
                model.ManifestName_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.ManifestName, storeScope);
                model.ManifestPictureId_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.ManifestPictureId, storeScope);
                model.ManifestShortName_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.ManifestShortName, storeScope);
                model.ManifestStartUrl_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.ManifestStartUrl, storeScope);
                model.ManifestThemeColor_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.ManifestThemeColor, storeScope);
                model.SoundFileUrl_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.SoundFileUrl, storeScope);
                model.VapidPrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.VapidPrivateKey, storeScope);
                model.VapidPublicKey_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.VapidPublicKey, storeScope);
                model.VapidSubjectEmail_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.VapidSubjectEmail, storeScope);
                model.Vibration_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.Vibration, storeScope);
                model.AbandonedCartCheckingOffset_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.AbandonedCartCheckingOffset, storeScope);
                model.UnitTypeId_OverrideForStore = await _settingService.SettingExistsAsync(webAppSettings, x => x.UnitTypeId, storeScope);


            }

            return model;
        }

        #endregion
    }
}
