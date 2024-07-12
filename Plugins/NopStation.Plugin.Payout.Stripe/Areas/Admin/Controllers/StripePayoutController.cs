using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using NopStation.Plugin.Misc.VendorCore.Areas.Admin;
using NopStation.Plugin.Payout.Stripe.Areas.Admin.Models;
using NopStation.Plugin.Payout.Stripe.Domain;
using NopStation.Plugin.Payout.Stripe.Services;
using NopStation.Plugin.Widgets.VendorCommission;

namespace NopStation.Plugin.Payout.Stripe.Areas.Admin.Controllers
{
    public class StripePayoutController : NopStationVendorController
    {
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorStripeConfigurationService _vendorStripeConfigurationService;

        #region Ctor

        public StripePayoutController(IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IVendorStripeConfigurationService vendorStripeConfigurationService)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _vendorStripeConfigurationService = vendorStripeConfigurationService;

        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StripePayoutPermissionProvider.ManageStripePayout))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var stripePayoutSettings = await _settingService.LoadSettingAsync<StripePayoutSettings>(storeScope);
            var vendorPayoutSettings = await _settingService.LoadSettingAsync<VendorPayoutSettings>(storeScope);
            var model = new ConfigurationModel
            {
                Active = stripePayoutSettings.Active && vendorPayoutSettings.ActivePayoutMethodSystemNames.Contains(StripeDefaults.SystemName),
                SecretKey = stripePayoutSettings.SecretKey,
                UseSandbox = stripePayoutSettings.UseSandbox,
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.Active_OverrideForStore = await _settingService.SettingExistsAsync(stripePayoutSettings, x => x.Active, storeScope);
                model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(stripePayoutSettings, x => x.SecretKey, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(stripePayoutSettings, x => x.UseSandbox, storeScope);
            }
            return View("~/Plugins/NopStation.Plugin.Payout.Stripe/Areas/Admin/Views/StripePayout/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripePayoutPermissionProvider.ManageStripePayout))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var stripePayoutSettings = await _settingService.LoadSettingAsync<StripePayoutSettings>(storeScope);
            var vendorPayoutSettings = await _settingService.LoadSettingAsync<VendorPayoutSettings>(storeScope);

            stripePayoutSettings.Active = model.Active;
            stripePayoutSettings.SecretKey = model.SecretKey;
            stripePayoutSettings.UseSandbox = model.UseSandbox;

            await _settingService.SaveSettingOverridablePerStoreAsync(stripePayoutSettings, x => x.Active, model.Active_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePayoutSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(stripePayoutSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            var isStripeActive = vendorPayoutSettings.ActivePayoutMethodSystemNames.Contains(StripeDefaults.SystemName);
            if (model.Active && !isStripeActive)
                vendorPayoutSettings.ActivePayoutMethodSystemNames.Add(StripeDefaults.SystemName);

            if (!model.Active && isStripeActive)
                vendorPayoutSettings.ActivePayoutMethodSystemNames.Remove(StripeDefaults.SystemName);

            await _settingService.SaveSettingOverridablePerStoreAsync(vendorPayoutSettings, x => x.ActivePayoutMethodSystemNames, model.Active_OverrideForStore, storeScope, false);
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            return await Configure();
        }

        #region Vendor Stripe Configuration

        public async Task<IActionResult> ConfigureVendorStripe(int vendorId)
        {
            if (!await _permissionService.AuthorizeAsync(StripePayoutPermissionProvider.ManageStripePayout))
                return AccessDeniedView();

            var vendorStripeConfiguration = await _vendorStripeConfigurationService.GetVendorStripeConfigurationByVendorIdAsync(vendorId);
            if (vendorStripeConfiguration != null)
            {
                var existingModel = new VendorStripeConfigurationModel
                {
                    VendorId = vendorStripeConfiguration.VendorId,
                    AccountId = vendorStripeConfiguration.AccountId,
                };
                return View(existingModel);
            }
            var model = new VendorStripeConfigurationModel
            {
                VendorId = vendorId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfigureVendorStripe(VendorStripeConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StripePayoutPermissionProvider.ManageStripePayout))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var configurationToUpdate = await _vendorStripeConfigurationService.GetVendorStripeConfigurationByVendorIdAsync(model.VendorId);
                if (configurationToUpdate != null)
                {
                    configurationToUpdate.AccountId = model.AccountId;

                    await _vendorStripeConfigurationService.UpdateVendorStripeConfigutationAsync(configurationToUpdate);

                    return Ok();
                }
                var configurationToCreate = new VendorStripeConfiguration
                {
                    VendorId = model.VendorId,
                    AccountId = model.AccountId,
                };
                await _vendorStripeConfigurationService.InsertVendorStripeConfigutationAsync(configurationToCreate);
                return Ok();
            }
            return Ok(new { Errors = GetErrorsFromModelState(ModelState) });
        }

        #endregion

        #endregion

        #region Utilities

        private IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
        {
            return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
        }

        #endregion
    }
}
