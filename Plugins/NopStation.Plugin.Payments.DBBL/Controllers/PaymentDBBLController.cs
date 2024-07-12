using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Payments.DBBL.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using System.Threading.Tasks;

namespace NopStation.Plugin.Payments.DBBL.Areas.Admin.Controllers
{
    public class PaymentDBBLController : BaseAdminController
    {
        #region Fields

        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PaymentDBBLController(INotificationService notificationService,
            ISettingService settingService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            IStoreContext storeContext)
        {
            _notificationService = notificationService;
            _settingService = settingService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(DBBLPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dbblSettings = await _settingService.LoadSettingAsync<DBBLPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UserId = dbblSettings.UserId,
                Password = dbblSettings.Password,
                UseSandbox = dbblSettings.UseSandbox,
                AdditionalFee = dbblSettings.AdditionalFee,
                AdditionalFeePercentage = dbblSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(dbblSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(dbblSettings, x => x.AdditionalFeePercentage, storeScope);
                model.UserId_OverrideForStore = await _settingService.SettingExistsAsync(dbblSettings, x => x.UserId, storeScope);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(dbblSettings, x => x.Password, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(dbblSettings, x => x.UseSandbox, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.DBBL/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DBBLPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dbblSettings = await _settingService.LoadSettingAsync<DBBLPaymentSettings>(storeScope);

            //save settings
            dbblSettings.UserId = model.UserId;
            dbblSettings.Password = model.Password;
            dbblSettings.UseSandbox = model.UseSandbox;
            dbblSettings.AdditionalFee = model.AdditionalFee;
            dbblSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(dbblSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dbblSettings, x => x.UserId, model.UserId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dbblSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dbblSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(dbblSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
