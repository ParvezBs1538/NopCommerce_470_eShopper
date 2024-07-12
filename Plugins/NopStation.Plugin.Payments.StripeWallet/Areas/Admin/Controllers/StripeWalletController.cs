using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.StripeWallet.Areas.Admin.Models;

namespace NopStation.Plugin.Payments.StripeWallet.Areas.Admin.Controllers
{
    public class StripeWalletController : NopStationAdminController
    {
        #region Field  

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly INopFileProvider _nopFileProvider;

        #endregion

        #region Ctor

        public StripeWalletController(
            ISettingService settingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService,
            INopFileProvider nopFileProvider)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _nopFileProvider = nopFileProvider;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StripeWalletsPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var stripeDigitalWalletSettings = await _settingService.LoadSettingAsync<StripeWalletSettings>(storeScope);

            var model = new ConfigurationModel
            {
                TransactionModeId = Convert.ToInt32(stripeDigitalWalletSettings.TransactionMode),
                AdditionalFee = stripeDigitalWalletSettings.AdditionalFee,
                AdditionalFeePercentage = stripeDigitalWalletSettings.AdditionalFeePercentage,
                TransactionModeValues = (await stripeDigitalWalletSettings.TransactionMode.ToSelectListAsync()).ToList(),
                ApiKey = stripeDigitalWalletSettings.SecretKey,
                ActiveStoreScopeConfiguration = storeScope,
                PublishableKey = stripeDigitalWalletSettings.PublishableKey
            };

            if (storeScope > 0)
            {
                model.TransactionModeId_OverrideForStore = await _settingService.SettingExistsAsync(stripeDigitalWalletSettings, x => x.TransactionMode, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(stripeDigitalWalletSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(stripeDigitalWalletSettings, x => x.AdditionalFeePercentage, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(stripeDigitalWalletSettings, x => x.SecretKey, storeScope);
                model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(stripeDigitalWalletSettings, x => x.PublishableKey, storeScope);
            }

            var filePath = _nopFileProvider.MapPath(string.Format(StripeDefaults.AppleVerificationFilePath, storeScope));
            model.AppleVerificationFileExist = _nopFileProvider.FileExists(filePath);

            if (!model.AppleVerificationFileExist)
                _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.NopStation.StripeWallet.Configuration.AppleVerificationFileNotExist"));

            return View("~/Plugins/NopStation.Plugin.Payments.StripeWallet/Areas/Admin/Views/StripeWallet/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripeWalletsPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var stripeDigitalWalletSettings = await _settingService.LoadSettingAsync<StripeWalletSettings>(storeScope);

            //save settings
            stripeDigitalWalletSettings.TransactionMode = (TransactionMode)model.TransactionModeId;
            stripeDigitalWalletSettings.AdditionalFee = model.AdditionalFee;
            stripeDigitalWalletSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            stripeDigitalWalletSettings.SecretKey = model.ApiKey;
            stripeDigitalWalletSettings.PublishableKey = model.PublishableKey;

            await _settingService.SaveSettingOverridablePerStoreAsync(stripeDigitalWalletSettings, x => x.TransactionMode, model.TransactionModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripeDigitalWalletSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripeDigitalWalletSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripeDigitalWalletSettings, x => x.SecretKey, model.ApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripeDigitalWalletSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
            return await Configure();
        }

        [EditAccess]
        public async Task<IActionResult> UploadFile(IFormFile verificationfile)
        {
            if (!await _permissionService.AuthorizeAsync(StripeWalletsPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (verificationfile != null && verificationfile.Length > 0)
            {
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var filePath = _nopFileProvider.MapPath(string.Format(StripeDefaults.AppleVerificationFilePath, storeScope));
                using Stream fileStream = new FileStream(filePath, FileMode.Create);
                await verificationfile.CopyToAsync(fileStream);
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.StripeWallet.Configuration.UnsuccessfulUpload"));
                return RedirectToAction("Configure");
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.StripeWallet.Configuration.SuccessfulUpload"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
