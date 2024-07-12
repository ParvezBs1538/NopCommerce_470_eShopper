using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Paykeeper.Models;

namespace NopStation.Plugin.Payments.Paykeeper.Controllers
{
    public class PaymentPaykeeperController : NopStationAdminController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public PaymentPaykeeperController(ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            INotificationService notificationService,
            IStoreContext storeContext,
            ILanguageService languageService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _languageService = languageService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(PaykeeperPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paykeeperPaymentSettings = await _settingService.LoadSettingAsync<PaykeeperPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                SecretWord = paykeeperPaymentSettings.SecretWord,
                GatewayUrl = paykeeperPaymentSettings.GatewayUrl,
                AdditionalFeePercentage = paykeeperPaymentSettings.AdditionalFeePercentage,
                AdditionalFee = paykeeperPaymentSettings.AdditionalFee,
                Login = paykeeperPaymentSettings.Login,
                Password = paykeeperPaymentSettings.Password,
                DescriptionText = paykeeperPaymentSettings.DescriptionText
            };

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(paykeeperPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(paykeeperPaymentSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(paykeeperPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(paykeeperPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.SecretWord_OverrideForStore = await _settingService.SettingExistsAsync(paykeeperPaymentSettings, x => x.SecretWord, storeScope);
                model.GatewayUrl_OverrideForStore = await _settingService.SettingExistsAsync(paykeeperPaymentSettings, x => x.GatewayUrl, storeScope);
                model.Login_OverrideForStore = await _settingService.SettingExistsAsync(paykeeperPaymentSettings, x => x.Login, storeScope);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(paykeeperPaymentSettings, x => x.Password, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.Paykeeper/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PaykeeperPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paykeeperPaymentSettings = await _settingService.LoadSettingAsync<PaykeeperPaymentSettings>(storeScope);

            //save settings
            paykeeperPaymentSettings.GatewayUrl = model.GatewayUrl;
            paykeeperPaymentSettings.SecretWord = model.SecretWord;
            paykeeperPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            paykeeperPaymentSettings.AdditionalFee = model.AdditionalFee;
            paykeeperPaymentSettings.Login = model.Login;
            paykeeperPaymentSettings.Password = model.Password;
            paykeeperPaymentSettings.DescriptionText = model.DescriptionText;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(paykeeperPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paykeeperPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paykeeperPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paykeeperPaymentSettings, x => x.SecretWord, model.SecretWord_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paykeeperPaymentSettings, x => x.Login, model.Login_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paykeeperPaymentSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paykeeperPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paykeeperPaymentSettings, x => x.GatewayUrl, model.GatewayUrl_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(paykeeperPaymentSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}