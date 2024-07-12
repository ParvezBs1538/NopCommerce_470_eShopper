using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Controllers
{
    public class RedxController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;

        #endregion

        #region ctor

        public RedxController(IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
        }

        #endregion ctor

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var redxSettings = await _settingService.LoadSettingAsync<RedxSettings>(storeId);

            var model = redxSettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return View(model);

            model.FlatShippingCharge_OverrideForStore = await _settingService.SettingExistsAsync(redxSettings, x => x.FlatShippingCharge, storeId);
            model.ApiAccessToken_OverrideForStore = await _settingService.SettingExistsAsync(redxSettings, x => x.ApiAccessToken, storeId);
            model.BaseUrl_OverrideForStore = await _settingService.SettingExistsAsync(redxSettings, x => x.BaseUrl, storeId);
            model.ParcelTrackUrl_OverrideForStore = await _settingService.SettingExistsAsync(redxSettings, x => x.ParcelTrackUrl, storeId);
            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(redxSettings, x => x.UseSandbox, storeId);
            model.SandboxUrl_OverrideForStore = await _settingService.SettingExistsAsync(redxSettings, x => x.SandboxUrl, storeId);
            model.ShipmentEventsUrl_OverrideForStore = await _settingService.SettingExistsAsync(redxSettings, x => x.ShipmentEventsUrl, storeId);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var redxSettings = await _settingService.LoadSettingAsync<RedxSettings>(storeScope);

            redxSettings = model.ToSettings(redxSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(redxSettings, x => x.FlatShippingCharge, model.FlatShippingCharge_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(redxSettings, x => x.ApiAccessToken, model.ApiAccessToken_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(redxSettings, x => x.BaseUrl, model.BaseUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(redxSettings, x => x.ParcelTrackUrl, model.ParcelTrackUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(redxSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(redxSettings, x => x.SandboxUrl, model.SandboxUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(redxSettings, x => x.ShipmentEventsUrl, model.ShipmentEventsUrl_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
