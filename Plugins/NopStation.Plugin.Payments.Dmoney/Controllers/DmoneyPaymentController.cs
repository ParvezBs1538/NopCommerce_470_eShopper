using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Dmoney.Models;

namespace NopStation.Plugin.Payments.Dmoney.Controllers
{
    public class DmoneyPaymentController : NopStationAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;

        public DmoneyPaymentController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(DmoneyPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dmoneySettings = await _settingService.LoadSettingAsync<DmoneyPaymentSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                BillerCode = dmoneySettings.BillerCode,
                GatewayUrl = dmoneySettings.GatewayUrl,
                OrganizationCode = dmoneySettings.OrganizationCode,
                Password = dmoneySettings.Password,
                SecretKey = dmoneySettings.SecretKey,
                TransactionVerificationUrl = dmoneySettings.TransactionVerificationUrl,
                Description = dmoneySettings.Description
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.BillerCode_OverrideForStore = await _settingService.SettingExistsAsync(dmoneySettings, x => x.BillerCode, storeScope);
                model.Description_OverrideForStore = await _settingService.SettingExistsAsync(dmoneySettings, x => x.Description, storeScope);
                model.GatewayUrl_OverrideForStore = await _settingService.SettingExistsAsync(dmoneySettings, x => x.GatewayUrl, storeScope);
                model.OrganizationCode_OverrideForStore = await _settingService.SettingExistsAsync(dmoneySettings, x => x.OrganizationCode, storeScope);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(dmoneySettings, x => x.Password, storeScope);
                model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(dmoneySettings, x => x.SecretKey, storeScope);
                model.TransactionVerificationUrl_OverrideForStore = await _settingService.SettingExistsAsync(dmoneySettings, x => x.TransactionVerificationUrl, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.Dmoney/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DmoneyPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dmoneySettings = await _settingService.LoadSettingAsync<DmoneyPaymentSettings>(storeScope);

            dmoneySettings.BillerCode = model.BillerCode;
            dmoneySettings.GatewayUrl = model.GatewayUrl;
            dmoneySettings.OrganizationCode = model.OrganizationCode;
            dmoneySettings.Password = model.Password;
            dmoneySettings.SecretKey = model.SecretKey;
            dmoneySettings.TransactionVerificationUrl = model.TransactionVerificationUrl;
            dmoneySettings.Description = model.Description;

            await _settingService.SaveSettingOverridablePerStoreAsync(dmoneySettings, x => x.BillerCode, model.BillerCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dmoneySettings, x => x.GatewayUrl, model.GatewayUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dmoneySettings, x => x.OrganizationCode, model.OrganizationCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dmoneySettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dmoneySettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dmoneySettings, x => x.TransactionVerificationUrl, model.TransactionVerificationUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dmoneySettings, x => x.Description, model.Description_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
