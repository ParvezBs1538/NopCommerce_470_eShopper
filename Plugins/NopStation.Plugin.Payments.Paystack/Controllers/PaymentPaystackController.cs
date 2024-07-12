using System.Linq;
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
using NopStation.Plugin.Payments.Paystack.Models;

namespace NopStation.Plugin.Payments.Paystack.Controllers
{
    public class PaymentPaystackController : NopStationAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;

        public PaymentPaystackController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPermissionService permissionService,
            ILanguageService languageService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _permissionService = permissionService;
            _languageService = languageService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(PaystackPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paystackPaymentSettings = await _settingService.LoadSettingAsync<PaystackPaymentSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                PublicKey = paystackPaymentSettings.PublicKey,
                SecretKey = paystackPaymentSettings.SecretKey,
                Description = paystackPaymentSettings.Description,
                AdditionalFeePercentage = paystackPaymentSettings.AdditionalFeePercentage,
                AdditionalFee = paystackPaymentSettings.AdditionalFee,
                Channels = paystackPaymentSettings.Channels,
                Currencies = paystackPaymentSettings.Currencies
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.Description = await _localizationService
                    .GetLocalizedSettingAsync(paystackPaymentSettings, x => x.Description, languageId, 0, false, false);
            });

            if (storeScope > 0)
            {
                model.Description_OverrideForStore = await _settingService.SettingExistsAsync(paystackPaymentSettings, x => x.Description, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(paystackPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(paystackPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublicKey_OverrideForStore = await _settingService.SettingExistsAsync(paystackPaymentSettings, x => x.PublicKey, storeScope);
                model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(paystackPaymentSettings, x => x.SecretKey, storeScope);
                model.Channels_OverrideForStore = await _settingService.SettingExistsAsync(paystackPaymentSettings, x => x.Channels, storeScope);
                model.Currencies_OverrideForStore = await _settingService.SettingExistsAsync(paystackPaymentSettings, x => x.Currencies, storeScope);
            }

            model.AvailableChannels.Add(new SelectListItem
            {
                Value = "card",
                Text = "Card"
            });
            model.AvailableChannels.Add(new SelectListItem
            {
                Value = "bank",
                Text = "Bank"
            });
            model.AvailableChannels.Add(new SelectListItem
            {
                Value = "ussd",
                Text = "USSD"
            });
            model.AvailableChannels.Add(new SelectListItem
            {
                Value = "qr",
                Text = "QR"
            });
            model.AvailableChannels.Add(new SelectListItem
            {
                Value = "mobile_money",
                Text = "Mobile money"
            });
            model.AvailableChannels.Add(new SelectListItem
            {
                Value = "bank_transfer",
                Text = "Bank transfer"
            });

            model.AvailableCurrencies.Add(new SelectListItem
            {
                Value = "GHS",
                Text = "GHS (Ghana)"
            });
            model.AvailableCurrencies.Add(new SelectListItem
            {
                Value = "NGN",
                Text = "NGN (Nigeria)"
            });
            model.AvailableCurrencies.Add(new SelectListItem
            {
                Value = "USD",
                Text = "USD (Nigeria)"
            });
            model.AvailableCurrencies.Add(new SelectListItem
            {
                Value = "ZAR",
                Text = "ZAR (South Africa)"
            });

            return View("~/Plugins/NopStation.Plugin.Payments.Paystack/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PaystackPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paystackPaymentSettings = await _settingService.LoadSettingAsync<PaystackPaymentSettings>(storeScope);

            paystackPaymentSettings.PublicKey = model.PublicKey;
            paystackPaymentSettings.SecretKey = model.SecretKey;
            paystackPaymentSettings.Description = model.Description;
            paystackPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            paystackPaymentSettings.AdditionalFee = model.AdditionalFee;
            paystackPaymentSettings.Channels = model.Channels.ToList();
            paystackPaymentSettings.Currencies = model.Currencies.ToList();

            await _settingService.SaveSettingOverridablePerStoreAsync(paystackPaymentSettings, x => x.PublicKey, model.PublicKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paystackPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paystackPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paystackPaymentSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paystackPaymentSettings, x => x.Description, model.Description_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paystackPaymentSettings, x => x.Channels, model.Channels_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paystackPaymentSettings, x => x.Currencies, model.Currencies_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(paystackPaymentSettings,
                    x => x.Description, localized.LanguageId, localized.Description);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
