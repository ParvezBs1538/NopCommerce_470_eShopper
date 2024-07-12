using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.bKash.Models;

namespace NopStation.Plugin.Payments.bKash.Controllers
{
    public class BkashPaymentController : NopStationAdminController
    {
        #region Field  

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctr

        public BkashPaymentController(ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreContext storeContext,
            IPermissionService permissionService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(BkashPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paymentSettings = await _settingService.LoadSettingAsync<BkashPaymentSettings>(storeScope);
            var model = paymentSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.AppKey_OverrideForStore = await _settingService.SettingExistsAsync(paymentSettings, x => x.AppKey, storeScope);
                model.AppSecret_OverrideForStore = await _settingService.SettingExistsAsync(paymentSettings, x => x.AppSecret, storeScope);
                model.Username_OverrideForStore = await _settingService.SettingExistsAsync(paymentSettings, x => x.Username, storeScope);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(paymentSettings, x => x.Password, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(paymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(paymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(paymentSettings, x => x.UseSandbox, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.bKash/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(BkashPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paymentSettings = await _settingService.LoadSettingAsync<BkashPaymentSettings>(storeScope);
            paymentSettings = model.ToSettings(paymentSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(paymentSettings, x => x.AppKey, model.AppKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paymentSettings, x => x.AppSecret, model.AppSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paymentSettings, x => x.Username, model.Username_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paymentSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
