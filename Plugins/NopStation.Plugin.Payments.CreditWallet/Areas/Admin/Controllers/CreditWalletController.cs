using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Controllers
{
    public class CreditWalletController : NopStationAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly ILanguageService _languageService;

        public CreditWalletController(IPermissionService permissionService,
            INotificationService notificationService,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            ILanguageService languageService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _currencyService = currencyService;
            _languageService = languageService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var creditWalletSettings = await _settingService.LoadSettingAsync<CreditWalletSettings>(storeScope);

            var model = new ConfigurationModel
            {
                AdditionalFee = creditWalletSettings.AdditionalFee,
                AdditionalFeePercentage = creditWalletSettings.AdditionalFeePercentage,
                DescriptionText = creditWalletSettings.DescriptionText,
                ShowInvoicesInCustomerWalletPage = creditWalletSettings.ShowInvoicesInCustomerWalletPage,
                MaxInvoicesToShowInCustomerWalletPage = creditWalletSettings.MaxInvoicesToShowInCustomerWalletPage,
                HideMethodIfInsufficientBalance = creditWalletSettings.HideMethodIfInsufficientBalance,
                SkipPaymentInfo = creditWalletSettings.SkipPaymentInfo,
                ShowAvailableCreditOnCheckoutPage = creditWalletSettings.ShowAvailableCreditOnCheckoutPage,
                ActiveStoreScopeConfiguration = storeScope
            };

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(creditWalletSettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            if (storeScope > 0)
            {
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(creditWalletSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(creditWalletSettings, x => x.AdditionalFeePercentage, storeScope);
                model.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(creditWalletSettings, x => x.DescriptionText, storeScope);
                model.SkipPaymentInfo_OverrideForStore = await _settingService.SettingExistsAsync(creditWalletSettings, x => x.SkipPaymentInfo, storeScope);
                model.ShowInvoicesInCustomerWalletPage_OverrideForStore = await _settingService.SettingExistsAsync(creditWalletSettings, x => x.ShowInvoicesInCustomerWalletPage, storeScope);
                model.MaxInvoicesToShowInCustomerWalletPage_OverrideForStore = await _settingService.SettingExistsAsync(creditWalletSettings, x => x.MaxInvoicesToShowInCustomerWalletPage, storeScope);
                model.ShowAvailableCreditOnCheckoutPage_OverrideForStore = await _settingService.SettingExistsAsync(creditWalletSettings, x => x.ShowAvailableCreditOnCheckoutPage, storeScope);
                model.HideMethodIfInsufficientBalance_OverrideForStore = await _settingService.SettingExistsAsync(creditWalletSettings, x => x.HideMethodIfInsufficientBalance, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.CreditWallet/Areas/Admin/Views/CreditWallet/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(CreditWalletPermissionProvider.ManageCustomerCreditPayment))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var creditWalletSettings = await _settingService.LoadSettingAsync<CreditWalletSettings>(storeScope);

            creditWalletSettings.AdditionalFee = model.AdditionalFee;
            creditWalletSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            creditWalletSettings.DescriptionText = model.DescriptionText;
            creditWalletSettings.SkipPaymentInfo = model.SkipPaymentInfo;
            creditWalletSettings.ShowInvoicesInCustomerWalletPage = model.ShowInvoicesInCustomerWalletPage;
            creditWalletSettings.MaxInvoicesToShowInCustomerWalletPage = model.MaxInvoicesToShowInCustomerWalletPage;
            creditWalletSettings.ShowAvailableCreditOnCheckoutPage = model.ShowAvailableCreditOnCheckoutPage;
            creditWalletSettings.HideMethodIfInsufficientBalance = model.HideMethodIfInsufficientBalance;

            await _settingService.SaveSettingOverridablePerStoreAsync(creditWalletSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(creditWalletSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(creditWalletSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(creditWalletSettings, x => x.SkipPaymentInfo, model.SkipPaymentInfo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(creditWalletSettings, x => x.ShowInvoicesInCustomerWalletPage, model.ShowInvoicesInCustomerWalletPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(creditWalletSettings, x => x.MaxInvoicesToShowInCustomerWalletPage, model.MaxInvoicesToShowInCustomerWalletPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(creditWalletSettings, x => x.ShowAvailableCreditOnCheckoutPage, model.ShowAvailableCreditOnCheckoutPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(creditWalletSettings, x => x.HideMethodIfInsufficientBalance, model.HideMethodIfInsufficientBalance_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(creditWalletSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}