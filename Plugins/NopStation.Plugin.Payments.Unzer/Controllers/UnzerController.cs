using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Unzer.Domain;
using NopStation.Plugin.Payments.Unzer.Models;

namespace NopStation.Plugin.Payments.Unzer.Controllers
{
    public class UnzerController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public UnzerController(ILocalizationService localizationService,
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
            if (!await _permissionService.AuthorizeAsync(UnzerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var unzerPaymentSettings = await _settingService.LoadSettingAsync<UnzerPaymentSettings>(storeScope);

            //prepare model
            var model = new ConfigurationModel
            {
                ApiPrivateKey = unzerPaymentSettings.ApiPrivateKey,
                ApiPublicKey = unzerPaymentSettings.ApiPublicKey,
                ApiEndpoint = unzerPaymentSettings.ApiEndpoint,
                ApiVersion = unzerPaymentSettings.ApiVersion,
                UseSandbox = unzerPaymentSettings.UseSandbox,
                TransactionModeId = (int)unzerPaymentSettings.TransactionMode,
                IsCardActive = unzerPaymentSettings.IsCardActive,
                IsPaypalActive = unzerPaymentSettings.IsPaypalActive,
                IsSofortActive = unzerPaymentSettings.IsSofortActive,
                IsEpsActive = unzerPaymentSettings.IsEpsActive,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.ApiPrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.ApiPrivateKey, storeScope);
                model.ApiPublicKey_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.ApiPublicKey, storeScope);
                model.ApiEndpoint_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.ApiEndpoint, storeScope);
                model.ApiVersion_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.ApiVersion, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.UseSandbox, storeScope);
                model.TransactionModeId_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.TransactionModeId, storeScope);
                model.IsCardActive_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.IsCardActive, storeScope);
                model.IsPaypalActive_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.IsPaypalActive, storeScope);
                model.IsSofortActive_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.IsSofortActive, storeScope);
                model.IsEpsActive_OverrideForStore = await _settingService.SettingExistsAsync(unzerPaymentSettings, x => x.IsEpsActive, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.Unzer/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(UnzerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var unzerPaymentSettings = await _settingService.LoadSettingAsync<UnzerPaymentSettings>(storeScope);

            unzerPaymentSettings.ApiPrivateKey = model.ApiPrivateKey;
            unzerPaymentSettings.ApiPublicKey = model.ApiPublicKey;
            unzerPaymentSettings.ApiEndpoint = model.ApiEndpoint;
            unzerPaymentSettings.ApiVersion = model.ApiVersion;
            unzerPaymentSettings.UseSandbox = model.UseSandbox;
            unzerPaymentSettings.TransactionMode = (TransactionMode)model.TransactionModeId;
            unzerPaymentSettings.IsCardActive = model.IsCardActive;
            unzerPaymentSettings.IsPaypalActive = model.IsPaypalActive;
            unzerPaymentSettings.IsSofortActive = model.IsSofortActive;
            unzerPaymentSettings.IsEpsActive = model.IsEpsActive;

            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.ApiPrivateKey, model.ApiPrivateKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.ApiPublicKey, model.ApiPublicKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.ApiEndpoint, model.ApiEndpoint_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.ApiVersion, model.ApiVersion_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.TransactionMode, model.TransactionModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.IsCardActive, model.IsCardActive_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.IsPaypalActive, model.IsPaypalActive_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.IsSofortActive, model.IsSofortActive_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(unzerPaymentSettings, x => x.IsEpsActive, model.IsEpsActive_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}