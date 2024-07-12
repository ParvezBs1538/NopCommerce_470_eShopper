using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Stripe.Models;

namespace NopStation.Plugin.Payments.Stripe.Controllers
{
    public class PaymentStripeController : NopStationAdminController
    {
        #region Field  
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        #endregion

        #region Ctor

        public PaymentStripeController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StripePaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var stripePaymentSettings = await _settingService.LoadSettingAsync<StripePaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                TransactionModeId = Convert.ToInt32(stripePaymentSettings.TransactionMode),
                AdditionalFee = stripePaymentSettings.AdditionalFee,
                AdditionalFeePercentage = stripePaymentSettings.AdditionalFeePercentage,
                TransactionModeValues = await stripePaymentSettings.TransactionMode.ToSelectListAsync(),
                ApiKey = stripePaymentSettings.ApiKey,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.TransactionModeId_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentSettings, x => x.TransactionMode, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentSettings, x => x.ApiKey, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Payments.Stripe/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripePaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var stripePaymentSettings = await _settingService.LoadSettingAsync<StripePaymentSettings>(storeScope);

            //save settings
            stripePaymentSettings.TransactionMode = (TransactionMode)model.TransactionModeId;
            stripePaymentSettings.AdditionalFee = model.AdditionalFee;
            stripePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            stripePaymentSettings.ApiKey = model.ApiKey;
            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            if (model.TransactionModeId_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(stripePaymentSettings, x => x.TransactionMode, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(stripePaymentSettings, x => x.TransactionMode, storeScope);

            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(stripePaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(stripePaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(stripePaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(stripePaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.ApiKey_OverrideForStore || storeScope == 0)
                await _settingService.SaveSettingAsync(stripePaymentSettings, x => x.ApiKey, storeScope, false);
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(stripePaymentSettings, x => x.ApiKey, storeScope);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
