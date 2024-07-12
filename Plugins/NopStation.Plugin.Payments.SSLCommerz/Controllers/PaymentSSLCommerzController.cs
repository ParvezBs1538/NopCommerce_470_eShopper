using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.SSLCommerz.Models;

namespace NopStation.Plugin.Payments.SSLCommerz.Controllers
{
    public class PaymentSSLCommerzController : NopStationAdminController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public PaymentSSLCommerzController(ISettingService settingService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            ILanguageService languageService)
        {
            _settingService = settingService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _languageService = languageService;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(SSLCommerzPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var commerzPaymentSettings = await _settingService.LoadSettingAsync<SSLCommerzPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = commerzPaymentSettings.UseSandbox,
                StoreID = commerzPaymentSettings.StoreID,
                Password = commerzPaymentSettings.Password,
                AdditionalFee = commerzPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = commerzPaymentSettings.AdditionalFeePercentage,
                PassProductNamesAndTotals = commerzPaymentSettings.PassProductNamesAndTotals,
                BusinessEmail = commerzPaymentSettings.BusinessEmail,
                ActiveStoreScopeConfiguration = storeScope,
                DescriptionText = commerzPaymentSettings.DescriptionText
            };

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(commerzPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(commerzPaymentSettings, x => x.DescriptionText, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(commerzPaymentSettings, x => x.UseSandbox, storeScope);
                model.StoreID_OverrideForStore = await _settingService.SettingExistsAsync(commerzPaymentSettings, x => x.StoreID, storeScope);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(commerzPaymentSettings, x => x.Password, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(commerzPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(commerzPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PassProductNamesAndTotals_OverrideForStore = await _settingService.SettingExistsAsync(commerzPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);
                model.BusinessEmail_OverrideForStore = await _settingService.SettingExistsAsync(commerzPaymentSettings, x => x.BusinessEmail, storeScope);
            }

            var shoppingCartSettings = await _settingService.LoadSettingAsync<ShoppingCartSettings>(storeScope);
            if (!shoppingCartSettings.RoundPricesDuringCalculation)
                _notificationService.WarningNotification(string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.SSLCommerz.RoundingWarning"), Url.Content("~/Admin/Setting/AllSettings")));

            return View("~/Plugins/NopStation.Plugin.Payments.SSLCommerz/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SSLCommerzPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var commerzPaymentSettings = await _settingService.LoadSettingAsync<SSLCommerzPaymentSettings>(storeScope);

            //save settings
            commerzPaymentSettings.DescriptionText = model.DescriptionText;
            commerzPaymentSettings.UseSandbox = model.UseSandbox;
            commerzPaymentSettings.StoreID = model.StoreID;
            commerzPaymentSettings.Password = model.Password;
            commerzPaymentSettings.AdditionalFee = model.AdditionalFee;
            commerzPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            commerzPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            commerzPaymentSettings.BusinessEmail = model.BusinessEmail;

            await _settingService.SaveSettingOverridablePerStoreAsync(commerzPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commerzPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commerzPaymentSettings, x => x.StoreID, model.StoreID_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commerzPaymentSettings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commerzPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commerzPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commerzPaymentSettings, x => x.PassProductNamesAndTotals, model.PassProductNamesAndTotals_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commerzPaymentSettings, x => x.BusinessEmail, model.BusinessEmail_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(commerzPaymentSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}