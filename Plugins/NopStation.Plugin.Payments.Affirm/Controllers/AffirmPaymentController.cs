using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Affirm.Domain;
using NopStation.Plugin.Payments.Affirm.Models;

namespace NopStation.Plugin.Payments.Affirm.Controllers
{
    public class AffirmPaymentController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public AffirmPaymentController(ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AffirmPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var affirmPaymentSettings = await _settingService.LoadSettingAsync<AffirmPaymentSettings>(storeScope);

            //prepare model
            var model = new ConfigurationModel
            {
                PublicApiKey = affirmPaymentSettings.PublicApiKey,
                PrivateApiKey = affirmPaymentSettings.PrivateApiKey,
                FinancialProductKey = affirmPaymentSettings.FinancialProductKey,
                UseSandbox = affirmPaymentSettings.UseSandbox,
                EnableOnProductBox = affirmPaymentSettings.EnableOnProductBox,
                CountryAPIModeId = (int)affirmPaymentSettings.CountryAPIMode,
                AvailableCountryAPIModes = await affirmPaymentSettings.CountryAPIMode.ToSelectListAsync(),
                TransactionModeId = (int)affirmPaymentSettings.TransactionMode,
                AvailableTransactionModes = await affirmPaymentSettings.TransactionMode.ToSelectListAsync(),
                AdditionalFee = affirmPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = affirmPaymentSettings.AdditionalFeePercentage,
                MerchantName = affirmPaymentSettings.MerchantName
            };

            if (storeScope > 0)
            {
                model.PublicApiKey_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.PublicApiKey, storeScope);
                model.PrivateApiKey_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.PrivateApiKey, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.UseSandbox, storeScope);
                model.EnableOnProductBox_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.EnableOnProductBox, storeScope);
                model.FinancialProductKey_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.FinancialProductKey, storeScope);
                model.CountryAPIModeId_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.CountryAPIMode, storeScope);
                model.TransactionModeId_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.TransactionMode, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.AdditionalFee, storeScope);
                model.MerchantName_OverrideForStore = await _settingService.SettingExistsAsync(affirmPaymentSettings, x => x.MerchantName, storeScope);
            }

            model.ActiveStoreScopeConfiguration = storeScope;

            return View("~/Plugins/NopStation.Plugin.Payments.Affirm/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AffirmPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var affirmPaymentSettings = await _settingService.LoadSettingAsync<AffirmPaymentSettings>(storeScope);

            affirmPaymentSettings.PublicApiKey = model.PublicApiKey;
            affirmPaymentSettings.PrivateApiKey = model.PrivateApiKey;
            affirmPaymentSettings.FinancialProductKey = model.FinancialProductKey;
            affirmPaymentSettings.UseSandbox = model.UseSandbox;
            affirmPaymentSettings.EnableOnProductBox = model.EnableOnProductBox;
            affirmPaymentSettings.CountryAPIMode = (CountryAPIMode)model.CountryAPIModeId;
            affirmPaymentSettings.TransactionMode = (TransactionMode)model.TransactionModeId;
            affirmPaymentSettings.AdditionalFee = model.AdditionalFee;
            affirmPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            affirmPaymentSettings.MerchantName = model.MerchantName;

            //save settings
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.PublicApiKey, model.PublicApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.PrivateApiKey, model.PrivateApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.EnableOnProductBox, model.EnableOnProductBox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.FinancialProductKey, model.FinancialProductKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.CountryAPIMode, model.CountryAPIModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.TransactionMode, model.TransactionModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(affirmPaymentSettings, x => x.MerchantName, model.MerchantName_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
