using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.NetsEasy.Areas.Admin.Models;
using NopStation.Plugin.Payments.NetsEasy.Enums;

namespace NopStation.Plugin.Payments.NetsEasy.Areas.Admin.Controllers
{
    public class NetsEasyPaymentController : NopStationAdminController
    {
        private readonly INotificationService _notificationService;
        private readonly ICountryService _countryService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public NetsEasyPaymentController(INotificationService notificationService,
            ICountryService countryService,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            _notificationService = notificationService;
            _countryService = countryService;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(NetsEasyPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                TestMode = netsEasyPaymentSettings.TestMode,
                TransactModeId = Convert.ToInt32(netsEasyPaymentSettings.TransactMode),
                AdditionalFee = netsEasyPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = netsEasyPaymentSettings.AdditionalFeePercentage,
                TransactModeValues = await netsEasyPaymentSettings.TransactMode.ToSelectListAsync(),
                CheckoutKey = netsEasyPaymentSettings.CheckoutKey,
                SecretKey = netsEasyPaymentSettings.SecretKey,
                ActiveStoreScopeConfiguration = storeScope,
                CheckoutPageUrl = netsEasyPaymentSettings.CheckoutPageUrl,
                ShowB2B = netsEasyPaymentSettings.ShowB2B,
                EnableLog = netsEasyPaymentSettings.EnableLog,
                EnsureRecurringInterval= netsEasyPaymentSettings.EnsureRecurringInterval
            };

            if (!string.IsNullOrEmpty(netsEasyPaymentSettings.LimitedToCountryIds))
            {
                model.SelectedCountryIds = netsEasyPaymentSettings.LimitedToCountryIds.Split(',').Select(int.Parse).ToList();
            }

            model.AvailableCountries = await (await _countryService.GetAllCountriesAsync()).Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = model.SelectedCountryIds.Any(id => id == x.Id)
            }).ToListAsync();

            if (storeScope > 0)
            {
                model.TestMode_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.TestMode, storeScope);
                model.CheckoutKey_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.CheckoutKey, storeScope);
                model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.SecretKey, storeScope);
                model.TransactModeId_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.TransactMode, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.ShowB2B_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.ShowB2B, storeScope);
                model.EnableLog_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.EnableLog, storeScope);
                model.CheckoutPageUrl_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.CheckoutPageUrl, storeScope);
                model.EnsureRecurringInterval_OverrideForStore = await _settingService.SettingExistsAsync(netsEasyPaymentSettings, x => x.EnsureRecurringInterval, storeScope);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> ConfigureAsync(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(NetsEasyPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeScope);

            //save settings
            netsEasyPaymentSettings.TestMode = model.TestMode;
            netsEasyPaymentSettings.TransactMode = (TransactMode)model.TransactModeId;
            netsEasyPaymentSettings.AdditionalFee = model.AdditionalFee;
            netsEasyPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            netsEasyPaymentSettings.CheckoutKey = model.CheckoutKey;
            netsEasyPaymentSettings.SecretKey = model.SecretKey;
            netsEasyPaymentSettings.LimitedToCountryIds = string.Join(",", model.SelectedCountryIds);
            netsEasyPaymentSettings.CheckoutPageUrl = model.CheckoutPageUrl;
            netsEasyPaymentSettings.ShowB2B = model.ShowB2B;
            netsEasyPaymentSettings.EnableLog = model.EnableLog;
            netsEasyPaymentSettings.EnsureRecurringInterval = model.EnsureRecurringInterval;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.EnsureRecurringInterval, model.EnsureRecurringInterval_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.TestMode, model.TestMode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.TransactMode, model.TransactModeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.CheckoutKey, model.CheckoutKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.LimitedToCountryIds, model.SelectedCountryIds_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.CheckoutPageUrl, model.CheckoutPageUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.ShowB2B, model.ShowB2B_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(netsEasyPaymentSettings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }
    }
}