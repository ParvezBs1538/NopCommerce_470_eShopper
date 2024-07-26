using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.GemVisaPayment.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Payments.GemVisaPayment.Areas.Admin.Controllers
{
    public class PaymentGemVisaController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        public PaymentGemVisaController(IPermissionService permissionService,
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
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var gemVisaPaymentSettings = _settingService.LoadSetting<GemVisaPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                UseSandbox = gemVisaPaymentSettings.UseSandbox,
                DebugMode = gemVisaPaymentSettings.DebugMode,
                MerchantId = gemVisaPaymentSettings.MerchantId,
                SecretKey = gemVisaPaymentSettings.SecretKey,
                MinimumAmount = gemVisaPaymentSettings.MinimumAmount,
                MaximumAmount = gemVisaPaymentSettings.MaximumAmount,
                ShowInfoInProductDetailsPage = gemVisaPaymentSettings.ShowInfoInProductDetailsPage,
                InstallmentMonth = gemVisaPaymentSettings.InstallmentMonth,
            };

            if (storeScope <= 0)
                return View(model);

            model.UseSandbox_OverrideForStore = _settingService.SettingExists(gemVisaPaymentSettings, x => x.UseSandbox, storeScope);
            model.DebugMode_OverrideForStore = _settingService.SettingExists(gemVisaPaymentSettings, x => x.DebugMode, storeScope);
            model.MerchantId_OverrideForStore = _settingService.SettingExists(gemVisaPaymentSettings, x => x.MerchantId, storeScope);
            model.SecretKey_OverrideForStore = _settingService.SettingExists(gemVisaPaymentSettings, x => x.SecretKey, storeScope);
            model.MinimumAmount_OverrideForStore = _settingService.SettingExists(gemVisaPaymentSettings, x => x.MinimumAmount, storeScope);
            model.MaximumAmount_OverrideForStore = _settingService.SettingExists(gemVisaPaymentSettings, x => x.MaximumAmount, storeScope);
            model.ShowInfoInProductDetailsPage_OverrideForStore = _settingService.SettingExists(gemVisaPaymentSettings, x => x.ShowInfoInProductDetailsPage, storeScope);
            model.InstallmentMonth_OverrideForStore = _settingService.SettingExists(gemVisaPaymentSettings, x => x.InstallmentMonth, storeScope);
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            var storeScope = model.ActiveStoreScopeConfiguration;
            //load settings for a chosen store scope
            var gemVisaPaymentSettings = _settingService.LoadSetting<GemVisaPaymentSettings>(storeScope);

            gemVisaPaymentSettings.UseSandbox = model.UseSandbox;
            gemVisaPaymentSettings.DebugMode = model.DebugMode;
            gemVisaPaymentSettings.MerchantId = model.MerchantId;
            gemVisaPaymentSettings.MinimumAmount = model.MinimumAmount;
            gemVisaPaymentSettings.MaximumAmount = model.MaximumAmount;
            gemVisaPaymentSettings.SecretKey = model.SecretKey;
            gemVisaPaymentSettings.ShowInfoInProductDetailsPage = model.ShowInfoInProductDetailsPage;
            gemVisaPaymentSettings.InstallmentMonth = model.InstallmentMonth;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(gemVisaPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gemVisaPaymentSettings, x => x.DebugMode, model.DebugMode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gemVisaPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gemVisaPaymentSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gemVisaPaymentSettings, x => x.MinimumAmount, model.MinimumAmount_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gemVisaPaymentSettings, x => x.MaximumAmount, model.MaximumAmount_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gemVisaPaymentSettings, x => x.ShowInfoInProductDetailsPage, model.ShowInfoInProductDetailsPage_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(gemVisaPaymentSettings, x => x.InstallmentMonth, model.InstallmentMonth_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}
