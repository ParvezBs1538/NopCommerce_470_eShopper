using System.Linq;
using System.Threading.Tasks;
using Amazon.Pay.API.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.AmazonPay.Models;

namespace NopStation.Plugin.Payments.AmazonPay.Controllers
{
    public class PaymentAmazonPayController : NopStationAdminController
    {
        #region Field

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public PaymentAmazonPayController(ISettingService settingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService,
            ILanguageService languageService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _languageService = languageService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var amazonPaySettings = await _settingService.LoadSettingAsync<AmazonPaySettings>(storeScope);

            var model = new ConfigurationModel()
            {
                AdditionalFee = amazonPaySettings.AdditionalFee,
                AdditionalFeePercentage = amazonPaySettings.AdditionalFeePercentage,
                DescriptionText = amazonPaySettings.DescriptionText,
                MerchantId = amazonPaySettings.MerchantId,
                ButtonColor = amazonPaySettings.ButtonColor,
                PublicKeyId = amazonPaySettings.PublicKeyId,
                NoteToBuyer = amazonPaySettings.NoteToBuyer,
                PrivateKey = amazonPaySettings.PrivateKey,
                UseSandbox = amazonPaySettings.UseSandbox,
                StoreId = amazonPaySettings.StoreId,
                RegionId = amazonPaySettings.RegionId,
                AvailableRegions = (await Region.UnitedStates.ToSelectListAsync()).ToList()
            };
            model.ActiveStoreScopeConfiguration = storeScope;

            model.AvailableButtonColors.Add(new SelectListItem(text: "Gold", value: "Gold"));
            model.AvailableButtonColors.Add(new SelectListItem(text: "Light gray", value: "LightGray"));
            model.AvailableButtonColors.Add(new SelectListItem(text: "Dark gray", value: "DarkGray"));

            //locales
            await AddLocalesAsync(_languageService, model.Locales, async (locale, languageId) =>
            {
                locale.DescriptionText = await _localizationService
                    .GetLocalizedSettingAsync(amazonPaySettings, x => x.DescriptionText, languageId, 0, false, false);
            });

            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.DescriptionText, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.UseSandbox, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.AdditionalFeePercentage, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.AdditionalFee, storeScope);
                model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.MerchantId, storeScope);
                model.PrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.PrivateKey, storeScope);
                model.PublicKeyId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.PublicKeyId, storeScope);
                model.NoteToBuyer_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.NoteToBuyer, storeScope);
                model.ButtonColor_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.ButtonColor, storeScope);
                model.StoreId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.StoreId, storeScope);
                model.RegionId_OverrideForStore = await _settingService.SettingExistsAsync(amazonPaySettings, x => x.RegionId, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.AmazonPay/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPayPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var stripePaymentSettings = await _settingService.LoadSettingAsync<AmazonPaySettings>(storeScope);

            stripePaymentSettings.DescriptionText = model.DescriptionText;
            stripePaymentSettings.UseSandbox = model.UseSandbox;
            stripePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            stripePaymentSettings.AdditionalFee = model.AdditionalFee;
            stripePaymentSettings.MerchantId = model.MerchantId;
            stripePaymentSettings.PrivateKey = model.PrivateKey;
            stripePaymentSettings.PublicKeyId = model.PublicKeyId;
            stripePaymentSettings.NoteToBuyer = model.NoteToBuyer;
            stripePaymentSettings.ButtonColor = model.ButtonColor;
            stripePaymentSettings.StoreId = model.StoreId;
            stripePaymentSettings.RegionId = model.RegionId;

            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.NoteToBuyer, model.NoteToBuyer_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.PrivateKey, model.PrivateKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.PublicKeyId, model.PublicKeyId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.ButtonColor, model.ButtonColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.StoreId, model.StoreId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentSettings, x => x.RegionId, model.RegionId_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedSettingAsync(stripePaymentSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
