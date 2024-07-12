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
using NopStation.Plugin.Payments.UpayBdManual.Models;

namespace NopStation.Plugin.Payments.UpayBdManual.Controllers
{
    public class UpayBdManualController : NopStationAdminController
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

        public UpayBdManualController(ILanguageService languageService,
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

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(UpayBdManualPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var upayBdManualSettings = await _settingService.LoadSettingAsync<UpayBdManualSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = upayBdManualSettings.DescriptionText
            };

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(upayBdManualSettings, x => x.DescriptionText, languageId, 0, false, false);
            });
            model.AdditionalFee = upayBdManualSettings.AdditionalFee;
            model.AdditionalFeePercentage = upayBdManualSettings.AdditionalFeePercentage;
            model.UpayNumber = upayBdManualSettings.UpayNumber;
            model.NumberType = upayBdManualSettings.NumberType;
            model.ValidatePhoneNumber = upayBdManualSettings.ValidatePhoneNumber;
            model.PhoneNumberRegex = upayBdManualSettings.PhoneNumberRegex;

            model.AvailableNumberTypes.Add(new SelectListItem() { Value = "Agent", Text = "Agent" });
            model.AvailableNumberTypes.Add(new SelectListItem() { Value = "Marchent", Text = "Marchent" });
            model.AvailableNumberTypes.Add(new SelectListItem() { Value = "Personal", Text = "Personal" });

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(upayBdManualSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(upayBdManualSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(upayBdManualSettings, x => x.AdditionalFeePercentage, storeScope);
                model.UpayNumber_OverrideForStore = await _settingService.SettingExistsAsync(upayBdManualSettings, x => x.UpayNumber, storeScope);
                model.NumberType_OverrideForStore = await _settingService.SettingExistsAsync(upayBdManualSettings, x => x.NumberType, storeScope);
                model.ValidatePhoneNumber_OverrideForStore = await _settingService.SettingExistsAsync(upayBdManualSettings, x => x.ValidatePhoneNumber, storeScope);
                model.PhoneNumberRegex_OverrideForStore = await _settingService.SettingExistsAsync(upayBdManualSettings, x => x.PhoneNumberRegex, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.UpayBdManual/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(UpayBdManualPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var upayBdManualSettings = await _settingService.LoadSettingAsync<UpayBdManualSettings>(storeScope);

            //save settings
            upayBdManualSettings.DescriptionText = model.DescriptionText;
            upayBdManualSettings.AdditionalFee = model.AdditionalFee;
            upayBdManualSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            upayBdManualSettings.UpayNumber = model.UpayNumber;
            upayBdManualSettings.NumberType = model.NumberType;
            upayBdManualSettings.ValidatePhoneNumber = model.ValidatePhoneNumber;
            upayBdManualSettings.PhoneNumberRegex = model.PhoneNumberRegex;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(upayBdManualSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(upayBdManualSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(upayBdManualSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(upayBdManualSettings, x => x.UpayNumber, model.UpayNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(upayBdManualSettings, x => x.NumberType, model.NumberType_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(upayBdManualSettings, x => x.ValidatePhoneNumber, model.ValidatePhoneNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(upayBdManualSettings, x => x.PhoneNumberRegex, model.PhoneNumberRegex_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(upayBdManualSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}