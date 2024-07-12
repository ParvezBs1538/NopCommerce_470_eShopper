using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.StripeBoleto.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Directory;

namespace NopStation.Plugin.Payments.StripeBoleto.Controllers
{
    public class PaymentStripeBoletoController : NopStationAdminController
    {
        #region Field  

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public PaymentStripeBoletoController(ISettingService settingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService,
            ICurrencyService currencyService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _currencyService = currencyService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StripeBoletoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var boletoPaymentSettings = await _settingService.LoadSettingAsync<StripeBoletoPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                AdditionalFee = boletoPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = boletoPaymentSettings.AdditionalFeePercentage,
                PublishableKey = boletoPaymentSettings.PublishableKey,
                ApiKey = boletoPaymentSettings.ApiKey,
                WebhookSecret = boletoPaymentSettings.WebhookSecret,
                SupportedCurrencyCodes = boletoPaymentSettings.SupportedCurrencyCodes,
                SendOrderInfoToStripe = boletoPaymentSettings.SendOrderInfoToStripe,
                ActiveStoreScopeConfiguration = storeScope
            };

            var currencies = await _currencyService.GetAllCurrenciesAsync(showHidden: true);
            model.AvailableCurrencyCodes = currencies.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CurrencyCode
            }).ToList();

            if (storeScope > 0)
            {
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(boletoPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(boletoPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(boletoPaymentSettings, x => x.PublishableKey, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(boletoPaymentSettings, x => x.ApiKey, storeScope);
                model.WebhookSecret_OverrideForStore = await _settingService.SettingExistsAsync(boletoPaymentSettings, x => x.WebhookSecret, storeScope);
                model.SupportedCurrencyCodes_OverrideForStore = await _settingService.SettingExistsAsync(boletoPaymentSettings, x => x.SupportedCurrencyCodes, storeScope);
                model.SendOrderInfoToStripe_OverrideForStore = await _settingService.SettingExistsAsync(boletoPaymentSettings, x => x.SendOrderInfoToStripe, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.StripeBoleto/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeBoletoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var boletoPaymentSettings = await _settingService.LoadSettingAsync<StripeBoletoPaymentSettings>(storeScope);

            //save settings
            boletoPaymentSettings.AdditionalFee = model.AdditionalFee;
            boletoPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            boletoPaymentSettings.ApiKey = model.ApiKey;
            boletoPaymentSettings.PublishableKey = model.PublishableKey;
            boletoPaymentSettings.WebhookSecret = model.WebhookSecret;
            boletoPaymentSettings.SupportedCurrencyCodes = model.SupportedCurrencyCodes.ToList();
            boletoPaymentSettings.SendOrderInfoToStripe = model.SendOrderInfoToStripe;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(boletoPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(boletoPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(boletoPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(boletoPaymentSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(boletoPaymentSettings, x => x.WebhookSecret, model.WebhookSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(boletoPaymentSettings, x => x.SupportedCurrencyCodes, model.SupportedCurrencyCodes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(boletoPaymentSettings, x => x.SendOrderInfoToStripe, model.SendOrderInfoToStripe_OverrideForStore, storeScope, false);
         
            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
