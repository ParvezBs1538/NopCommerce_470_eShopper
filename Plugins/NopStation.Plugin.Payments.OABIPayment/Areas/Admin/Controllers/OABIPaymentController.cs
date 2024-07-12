using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.OABIPayment.Models;

namespace NopStation.Plugin.Payments.OABIPayment.Areas.Admin.Controllers
{
    public class OABIPaymentController : NopStationAdminController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public OABIPaymentController(ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            INotificationService notificationService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(OABIPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var oabIPaymentSettings = await _settingService.LoadSettingAsync<OABIPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                TranPortaPassword = oabIPaymentSettings.TranPortaPassword,
                AdditionalFeeInPercentage = oabIPaymentSettings.AdditionalFeeInPercentage,
                AdditionalFee = oabIPaymentSettings.AdditionalFee,
                TranPortalId = oabIPaymentSettings.TranPortalId,
                ResourceKey = oabIPaymentSettings.ResourceKey,
            };

            if (storeScope <= 0)
                return View("~/Plugins/NopStation.Plugin.Payments.OABIPayment/Areas/Admin/Views/Configure.cshtml", model);

            model.TranPortaPassword_OverrideForStore = await _settingService.SettingExistsAsync(oabIPaymentSettings, x => x.TranPortaPassword, storeScope);
            model.TranPortalId_OverrideForStore = await _settingService.SettingExistsAsync(oabIPaymentSettings, x => x.TranPortalId, storeScope);
            model.ResourceKey_OverrideForStore = await _settingService.SettingExistsAsync(oabIPaymentSettings, x => x.ResourceKey, storeScope);
            model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(oabIPaymentSettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeeInPercentage_OverrideForStore = await _settingService.SettingExistsAsync(oabIPaymentSettings, x => x.AdditionalFeeInPercentage, storeScope);

            return View("~/Plugins/NopStation.Plugin.Payments.OABIPayment/Areas/Admin/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(OABIPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var oabIPaymentSettings = await _settingService.LoadSettingAsync<OABIPaymentSettings>(storeScope);
            //save settings
            oabIPaymentSettings.TranPortalId = model.TranPortalId;
            oabIPaymentSettings.TranPortaPassword = model.TranPortaPassword;
            oabIPaymentSettings.ResourceKey = model.ResourceKey;
            oabIPaymentSettings.AdditionalFeeInPercentage = model.AdditionalFeeInPercentage;
            oabIPaymentSettings.AdditionalFee = model.AdditionalFee;

            await _settingService.SaveSettingOverridablePerStoreAsync(oabIPaymentSettings, x => x.TranPortalId, model.TranPortalId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oabIPaymentSettings, x => x.TranPortaPassword, model.TranPortaPassword_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oabIPaymentSettings, x => x.ResourceKey, model.ResourceKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oabIPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(oabIPaymentSettings, x => x.AdditionalFeeInPercentage, model.AdditionalFeeInPercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}