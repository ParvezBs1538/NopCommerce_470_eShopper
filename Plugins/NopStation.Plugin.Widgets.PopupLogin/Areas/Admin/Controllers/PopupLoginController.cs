using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using NopStation.Plugin.Widgets.PopupLogin.Areas.Admin.Models;
using Nop.Core;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Services.Security;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Widgets.PopupLogin.Areas.Admin.Controllers
{
    public partial class PopupLoginController : NopStationAdminController
    {
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        public PopupLoginController(IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(PopupLoginPermissionProvider.ManagePopupLogin))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var popupLoginSettings = await _settingService.LoadSettingAsync<PopupLoginSettings>(storeId);

            var model = popupLoginSettings.ToSettingsModel<ConfigurationModel>();
            
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return View(model);

            model.EnablePopupLogin_OverrideForStore = await _settingService.SettingExistsAsync(popupLoginSettings, x => x.EnablePopupLogin, storeId);
            model.LoginUrlElementSelector_OverrideForStore = await _settingService.SettingExistsAsync(popupLoginSettings, x => x.LoginUrlElementSelector, storeId);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PopupLoginPermissionProvider.ManagePopupLogin))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var popupLoginSettings = await _settingService.LoadSettingAsync<PopupLoginSettings>(storeId);
            popupLoginSettings = model.ToSettings(popupLoginSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(popupLoginSettings, x => x.EnablePopupLogin, model.EnablePopupLogin_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(popupLoginSettings, x => x.LoginUrlElementSelector, model.LoginUrlElementSelector_OverrideForStore, storeId, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
