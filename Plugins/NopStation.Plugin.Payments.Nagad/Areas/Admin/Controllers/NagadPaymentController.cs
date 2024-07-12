using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Nagad;
using NopStation.Plugin.Payments.Nagad.Areas.Admin.Models;

namespace NopStation.Plugin.Payments.Afterpay.Areas.Admin.Controllers
{
    public class NagadPaymentController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public NagadPaymentController(IPermissionService permissionService,
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

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(NagadPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var nagadPaymentSettings = await _settingService.LoadSettingAsync<NagadPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                Description = nagadPaymentSettings.Description,
                UseSandbox = nagadPaymentSettings.UseSandbox,
                MerchantId = nagadPaymentSettings.MerchantId,
                NPGPublicKey = nagadPaymentSettings.NPGPublicKey,
                MSPrivateKey = nagadPaymentSettings.MSPrivateKey,
                AdditionalFee = nagadPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = nagadPaymentSettings.AdditionalFeePercentage
            };

            if (storeScope <= 0)
                return View("~/Plugins/NopStation.Plugin.Payments.Nagad/Areas/Admin/Views/NagadPayment/Configure.cshtml", model);

            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(nagadPaymentSettings, x => x.UseSandbox, storeScope);
            model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(nagadPaymentSettings, x => x.MerchantId, storeScope);
            model.NPGPublicKey_OverrideForStore = await _settingService.SettingExistsAsync(nagadPaymentSettings, x => x.NPGPublicKey, storeScope);
            model.MSPrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(nagadPaymentSettings, x => x.MSPrivateKey, storeScope);
            model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(nagadPaymentSettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(nagadPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            model.Description_OverrideForStore = await _settingService.SettingExistsAsync(nagadPaymentSettings, x => x.Description, storeScope);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(NagadPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            var storeScope = model.ActiveStoreScopeConfiguration;

            var nagadPaymentSettings = await _settingService.LoadSettingAsync<NagadPaymentSettings>(storeScope);

            nagadPaymentSettings.UseSandbox = model.UseSandbox;
            nagadPaymentSettings.MerchantId = model.MerchantId;
            nagadPaymentSettings.NPGPublicKey = model.NPGPublicKey;
            nagadPaymentSettings.MSPrivateKey = model.MSPrivateKey;
            nagadPaymentSettings.AdditionalFee = model.AdditionalFee;
            nagadPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            nagadPaymentSettings.Description = model.Description;

            await _settingService.SaveSettingOverridablePerStoreAsync(nagadPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadPaymentSettings, x => x.NPGPublicKey, model.NPGPublicKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadPaymentSettings, x => x.MSPrivateKey, model.MSPrivateKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadPaymentSettings, x => x.Description, model.Description_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Nagad.Configuration.Saved"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
