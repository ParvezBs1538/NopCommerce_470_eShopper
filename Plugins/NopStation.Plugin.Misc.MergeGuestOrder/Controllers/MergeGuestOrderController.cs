using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.MergeGuestOrder;
using NopStation.Plugin.Misc.MergeGuestOrder.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.MergeGuestOrder.Controllers
{
    public class MergeGuestOrderController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public MergeGuestOrderController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(MergeGuestOrderPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mergeGuestOrderSettings = await _settingService.LoadSettingAsync<MergeGuestOrderSettings>(storeScope);

            var model = new ConfigurationModel
            {
                AddNoteToOrderOnMerge = mergeGuestOrderSettings.AddNoteToOrderOnMerge,
                CheckEmailInAddressId = mergeGuestOrderSettings.CheckEmailInAddressId,
                EnablePlugin = mergeGuestOrderSettings.EnablePlugin,
                ActiveStoreScopeConfiguration = storeScope
            };

            model.AvailableCheckEmailOptions = (await CheckEmailInAddress.Both.ToSelectListAsync()).ToList();

            if (storeScope > 0)
            {
                model.AddNoteToOrderOnMerge_OverrideForStore = await _settingService.SettingExistsAsync(mergeGuestOrderSettings, x => x.AddNoteToOrderOnMerge, storeScope);
                model.CheckEmailInAddressId_OverrideForStore = await _settingService.SettingExistsAsync(mergeGuestOrderSettings, x => x.CheckEmailInAddressId, storeScope);
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(mergeGuestOrderSettings, x => x.EnablePlugin, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Misc.MergeGuestOrder/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(MergeGuestOrderPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mergeGuestOrderSettings = await _settingService.LoadSettingAsync<MergeGuestOrderSettings>(storeScope);

            //save settings
            mergeGuestOrderSettings.AddNoteToOrderOnMerge = model.AddNoteToOrderOnMerge;
            mergeGuestOrderSettings.CheckEmailInAddressId = model.CheckEmailInAddressId;
            mergeGuestOrderSettings.EnablePlugin = model.EnablePlugin;

            await _settingService.SaveSettingOverridablePerStoreAsync(mergeGuestOrderSettings, x => x.AddNoteToOrderOnMerge, model.AddNoteToOrderOnMerge_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mergeGuestOrderSettings, x => x.CheckEmailInAddressId, model.CheckEmailInAddressId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mergeGuestOrderSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}