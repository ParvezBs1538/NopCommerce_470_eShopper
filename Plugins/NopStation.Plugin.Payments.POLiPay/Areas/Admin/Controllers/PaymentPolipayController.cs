using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.POLiPay.Areas.Admin.Models;

namespace NopStation.Plugin.Payments.POLiPay.Areas.Admin.Controllers
{
    public class PaymentPolipayController : NopStationAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        public PaymentPolipayController(IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(PoliPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var poliPaySettings = await _settingService.LoadSettingAsync<PoliPaySettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = poliPaySettings.UseSandbox,
                MerchantCode = poliPaySettings.MerchantCode,
                AuthCode = poliPaySettings.AuthCode,
                AdditionalFee = poliPaySettings.AdditionalFee,
                AdditionalFeePercentage = poliPaySettings.AdditionalFeePercentage,
            };

            if (storeScope <= 0)
                return View("~/Plugins/NopStation.Plugin.Payments.POLiPay/Areas/Admin/Views/PaymentPolipay/Configure.cshtml", model);

            model.MerchantCode_OverrideForStore = await _settingService.SettingExistsAsync(poliPaySettings, x => x.MerchantCode, storeScope);
            model.AuthCode_OverrideForStore = await _settingService.SettingExistsAsync(poliPaySettings, x => x.AuthCode, storeScope);
            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(poliPaySettings, x => x.UseSandbox, storeScope);
            model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(poliPaySettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(poliPaySettings, x => x.AdditionalFeePercentage, storeScope);

            return View("~/Plugins/NopStation.Plugin.Payments.POLiPay/Areas/Admin/Views/PaymentPolipay/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PoliPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            //load settings for a chosen store scope
            var poliPaySettings = await _settingService.LoadSettingAsync<PoliPaySettings>(storeScope);

            poliPaySettings.MerchantCode = model.MerchantCode;
            poliPaySettings.AuthCode = model.AuthCode;
            poliPaySettings.UseSandbox = model.UseSandbox;
            poliPaySettings.AdditionalFee = model.AdditionalFee;
            poliPaySettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(poliPaySettings, x => x.MerchantCode, model.MerchantCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(poliPaySettings, x => x.AuthCode, model.AuthCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(poliPaySettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(poliPaySettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(poliPaySettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
