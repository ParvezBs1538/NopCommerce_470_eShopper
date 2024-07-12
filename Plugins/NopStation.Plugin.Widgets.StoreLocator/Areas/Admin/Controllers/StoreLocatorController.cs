using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Controllers
{
    public class StoreLocatorController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public StoreLocatorController(ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        #region Configure

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocatorConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var storeLocatorSettings = await _settingService.LoadSettingAsync<StoreLocatorSettings>(storeScope);
             
            var model = storeLocatorSettings.ToSettingsModel<ConfigurationModel>();
            model.AvailableDistanceCalculationMethods = (await DistanceCalculationMethod.GeoCoordinate.ToSelectListAsync()).ToList();
            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope == 0)
                return View(model);

            model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.EnablePlugin, storeScope);
            model.GoogleMapApiKey_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.GoogleMapApiKey, storeScope);
            model.SortPickupPointsByDistance_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.SortPickupPointsByDistance, storeScope);
            model.DistanceCalculationMethodId_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.DistanceCalculationMethodId, storeScope);
            model.GoogleDistanceMatrixApiKey_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.GoogleDistanceMatrixApiKey, storeScope);
            model.IncludeInTopMenu_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.IncludeInTopMenu, storeScope);
            model.HideInMobileView_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.HideInMobileView, storeScope);
            model.IncludeInFooterColumn_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.IncludeInFooterColumn, storeScope);
            model.FooterColumnSelector_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.FooterColumnSelector, storeScope);
            model.PublicDispalyPageSize_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.PublicDispalyPageSize, storeScope);
            model.PictureSize_OverrideForStore = await _settingService.SettingExistsAsync(storeLocatorSettings, x => x.PictureSize, storeScope);

            return View(model);
        }

        [CheckAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocatorConfiguration))
                return AccessDeniedView();
            
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var storeLocatorSettings = await _settingService.LoadSettingAsync<StoreLocatorSettings>(storeScope);
             
            storeLocatorSettings = model.ToSettings(storeLocatorSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.GoogleMapApiKey, model.GoogleMapApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.SortPickupPointsByDistance, model.SortPickupPointsByDistance_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.DistanceCalculationMethodId, model.DistanceCalculationMethodId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.GoogleDistanceMatrixApiKey, model.GoogleDistanceMatrixApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.IncludeInTopMenu, model.IncludeInTopMenu_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.HideInMobileView, model.HideInMobileView_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.IncludeInFooterColumn, model.IncludeInFooterColumn_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.FooterColumnSelector, model.FooterColumnSelector_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.PublicDispalyPageSize, model.PublicDispalyPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeLocatorSettings, x => x.PictureSize, model.PictureSize_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #endregion
    }
}
