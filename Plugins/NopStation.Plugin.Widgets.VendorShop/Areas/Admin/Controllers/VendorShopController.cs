using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Themes;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.VendorShop.Domains;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Controllers
{
    public class VendorShopController : NopStationAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorProfileModelFactory _vendorProfileModelFactory;
        private readonly IVendorProfileService _vendorProfileService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IThemeContext _themeContext;
        private readonly INopFileProvider _nopFileProvider;
        private readonly ILogger _logger;
        private readonly IVendorShopFeatureService _vendorShopFeatureService;

        public VendorShopController(IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IVendorProfileModelFactory vendorProfileModelFactory,
            IVendorProfileService vendorProfileService,
            IWorkContext workContext,
            ILocalizedEntityService localizedEntityService,
            IThemeContext themeContext,
            INopFileProvider nopFileProvider,
            ILogger logger,
            IVendorShopFeatureService vendorShopFeatureService)
        {
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _vendorProfileModelFactory = vendorProfileModelFactory;
            _vendorProfileService = vendorProfileService;
            _workContext = workContext;
            _localizedEntityService = localizedEntityService;
            _themeContext = themeContext;
            _nopFileProvider = nopFileProvider;
            _logger = logger;
            _vendorShopFeatureService = vendorShopFeatureService;
        }

        #region Configure

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var vendorShopSettings = await _settingService.LoadSettingAsync<VendorShopSettings>(storeId);

            var model = vendorShopSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return View(model);

            model.EnableOCarousel_OverrideForStore = await _settingService.SettingExistsAsync(vendorShopSettings, x => x.EnableOCarousel, storeId);
            model.EnableSlider_OverrideForStore = await _settingService.SettingExistsAsync(vendorShopSettings, x => x.EnableSlider, storeId);
            model.EnableProductTabs_OverrideForStore = await _settingService.SettingExistsAsync(vendorShopSettings, x => x.EnableProductTabs, storeId);
            model.EnableVendorShopCampaign_OverrideForStore = await _settingService.SettingExistsAsync(vendorShopSettings, x => x.EnableVendorShopCampaign, storeId);
            model.EnableVendorCustomCss_OverrideForStore = await _settingService.SettingExistsAsync(vendorShopSettings, x => x.EnableVendorCustomCss, storeId);


            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var vendorShopSettings = await _settingService.LoadSettingAsync<VendorShopSettings>(storeScope);
            vendorShopSettings = model.ToSettings(vendorShopSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(vendorShopSettings, x => x.EnableOCarousel, model.EnableOCarousel_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorShopSettings, x => x.EnableSlider, model.EnableSlider_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorShopSettings, x => x.EnableProductTabs, model.EnableProductTabs_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorShopSettings, x => x.EnableVendorShopCampaign, model.EnableVendorShopCampaign_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorShopSettings, x => x.EnableVendorCustomCss, model.EnableVendorCustomCss_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> Profile()
        {
            if (!await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageVendorProfile))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null || !await _vendorShopFeatureService.IsEnableVendorShopAsync(currentVendor.Id))
                return AccessDeniedView();

            var vendorProfile = await _vendorProfileService.GetVendorProfileAsync(currentVendor.Id, storeScope);

            var vendorProfileModel = await _vendorProfileModelFactory.PrepareVendorProfileModelAsync(new VendorProfileModel(), vendorProfile);

            vendorProfileModel.ActiveStoreScopeConfiguration = storeScope;

            return View(vendorProfileModel);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(VendorProfileModel vendorProfileModel)
        {
            if (!await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageVendorProfile))
                return AccessDeniedView();

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null || !await _vendorShopFeatureService.IsEnableVendorShopAsync(currentVendor.Id))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var vendorProfile = await _vendorProfileService.GetVendorProfileAsync(currentVendor.Id, storeScope);
            var profileId = vendorProfile?.Id ?? 0;
            vendorProfile = vendorProfileModel.ToEntity<VendorProfile>();
            vendorProfile.Id = profileId;

            vendorProfile.StoreId = storeScope;
            vendorProfile.VendorId = currentVendor.Id;

            await _vendorProfileService.SaveVendorProfileAsync(vendorProfile);

            await UpdateVendorProfileLocalesAsync(vendorProfile, vendorProfileModel);
            return RedirectToAction(nameof(Profile));
        }

        protected virtual async Task UpdateVendorProfileLocalesAsync(VendorProfile vendorProfile, VendorProfileModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(vendorProfile,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);
            }
        }
        #endregion
    }
}
