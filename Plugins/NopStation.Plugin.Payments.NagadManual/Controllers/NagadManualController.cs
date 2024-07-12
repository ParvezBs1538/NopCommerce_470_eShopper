using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.NagadManual.Models;

namespace NopStation.Plugin.Payments.NagadManual.Controllers
{
    public class NagadManualController : NopStationAdminController
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public NagadManualController(ILanguageService languageService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(NagadManualPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var nagadManualPaymentSettings = await _settingService.LoadSettingAsync<NagadManualPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = nagadManualPaymentSettings.DescriptionText
            };

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(nagadManualPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });
            model.AdditionalFee = nagadManualPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = nagadManualPaymentSettings.AdditionalFeePercentage;
            model.NagadNumber = nagadManualPaymentSettings.NagadNumber;
            model.NumberType = nagadManualPaymentSettings.NumberType;
            model.ValidatePhoneNumber = nagadManualPaymentSettings.ValidatePhoneNumber;
            model.PhoneNumberRegex = nagadManualPaymentSettings.PhoneNumberRegex;

            model.AvailableNumberTypes.Add(new SelectListItem() { Value = "Agent", Text = "Agent" });
            model.AvailableNumberTypes.Add(new SelectListItem() { Value = "Merchant", Text = "Merchant" });
            model.AvailableNumberTypes.Add(new SelectListItem() { Value = "Personal", Text = "Personal" });

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(nagadManualPaymentSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(nagadManualPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(nagadManualPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.NagadNumber_OverrideForStore = await _settingService.SettingExistsAsync(nagadManualPaymentSettings, x => x.NagadNumber, storeScope);
                model.NumberType_OverrideForStore = await _settingService.SettingExistsAsync(nagadManualPaymentSettings, x => x.NumberType, storeScope);
                model.ValidatePhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(nagadManualPaymentSettings, x => x.ValidatePhoneNumber, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(nagadManualPaymentSettings, x => x.PhoneNumberRegex, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.NagadManual/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(NagadManualPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var nagadManualPaymentSettings = await _settingService.LoadSettingAsync<NagadManualPaymentSettings>(storeScope);

            //save settings
            nagadManualPaymentSettings.DescriptionText = model.DescriptionText;
            nagadManualPaymentSettings.AdditionalFee = model.AdditionalFee;
            nagadManualPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            nagadManualPaymentSettings.NagadNumber = model.NagadNumber;
            nagadManualPaymentSettings.NumberType = model.NumberType;
            nagadManualPaymentSettings.ValidatePhoneNumber = model.ValidatePhoneNumber;
            nagadManualPaymentSettings.PhoneNumberRegex = model.PhoneNumberRegex;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadManualPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadManualPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadManualPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadManualPaymentSettings, x => x.NagadNumber, model.NagadNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadManualPaymentSettings, x => x.NumberType, model.NumberType_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadManualPaymentSettings, x => x.ValidatePhoneNumber, model.ValidatePhoneNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nagadManualPaymentSettings, x => x.PhoneNumberRegex, model.PhoneNumberRegex_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(nagadManualPaymentSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}