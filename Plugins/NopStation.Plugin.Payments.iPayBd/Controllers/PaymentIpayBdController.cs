using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.iPayBd.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.Payments.iPayBd.Controllers
{
    public class PaymentIpayBdController : NopStationAdminController
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

        public PaymentIpayBdController(ILanguageService languageService,
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
            if (!await _permissionService.AuthorizeAsync(IpayBdPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ipayBdPaymentSettings = await _settingService.LoadSettingAsync<IpayBdPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = ipayBdPaymentSettings.DescriptionText
            };

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(ipayBdPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });
            model.AdditionalFee = ipayBdPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = ipayBdPaymentSettings.AdditionalFeePercentage;
            model.ApiKey = ipayBdPaymentSettings.ApiKey;
            model.Sandbox = ipayBdPaymentSettings.Sandbox;

            model.ActiveStoreScopeConfiguration = storeScope;

            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(ipayBdPaymentSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(ipayBdPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(ipayBdPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(ipayBdPaymentSettings, x => x.ApiKey, storeScope);
                model.Sandbox_OverrideForStore = await _settingService.SettingExistsAsync(ipayBdPaymentSettings, x => x.Sandbox, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.iPayBd/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(IpayBdPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ipayBdPaymentSettings = await _settingService.LoadSettingAsync<IpayBdPaymentSettings>(storeScope);

            //save settings
            ipayBdPaymentSettings.DescriptionText = model.DescriptionText;
            ipayBdPaymentSettings.AdditionalFee = model.AdditionalFee;
            ipayBdPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            ipayBdPaymentSettings.ApiKey = model.ApiKey;
            ipayBdPaymentSettings.Sandbox = model.Sandbox;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(ipayBdPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ipayBdPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ipayBdPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ipayBdPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ipayBdPaymentSettings, x => x.Sandbox, model.Sandbox_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(ipayBdPaymentSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}