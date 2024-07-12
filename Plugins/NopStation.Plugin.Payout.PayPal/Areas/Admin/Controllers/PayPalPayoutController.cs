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
using NopStation.Plugin.Payout.PayPal;
using NopStation.Plugin.Payout.PayPal.Areas.Admin.Models;
using NopStation.Plugin.Payout.PayPal.Domain;
using NopStation.Plugin.Payout.PayPal.Services;
using NopStation.Plugin.Widgets.VendorCommission;

namespace NopStation.Plugin.Payout.Stripe.Areas.Admin.Controllers
{
    public class PayPalPayoutController : NopStationVendorController
    {
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorPayPalConfigurationService _vendorPayPalConfigurationService;


        public PayPalPayoutController(IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IVendorPayPalConfigurationService vendorPayPalConfigurationService)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _vendorPayPalConfigurationService = vendorPayPalConfigurationService;
        }

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(PayPalPayoutPermissionProvider.ManagePayPalPayout))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paypalPayoutSettings = await _settingService.LoadSettingAsync<PayPalPayoutSettings>(storeScope);
            var vendorPayoutSettings = await _settingService.LoadSettingAsync<VendorPayoutSettings>(storeScope);
            var model = new ConfigurationModel
            {
                Active = paypalPayoutSettings.Active && vendorPayoutSettings.ActivePayoutMethodSystemNames.Contains(PayPalDefaults.SystemName),
                ClientId = paypalPayoutSettings.ClientId,
                SecretKey = paypalPayoutSettings.SecretKey,
                UseSandbox = paypalPayoutSettings.UseSandbox,
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.Active_OverrideForStore = await _settingService.SettingExistsAsync(paypalPayoutSettings, x => x.Active, storeScope);
                model.ClientId_OverrideForStore = await _settingService.SettingExistsAsync(paypalPayoutSettings, x => x.ClientId, storeScope);
                model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(paypalPayoutSettings, x => x.SecretKey, storeScope);
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(paypalPayoutSettings, x => x.UseSandbox, storeScope);
            }
            return View("~/Plugins/NopStation.Plugin.Payout.PayPal/Areas/Admin/Views/PayPalPayout/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PayPalPayoutPermissionProvider.ManagePayPalPayout))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var paypalPayoutSettings = await _settingService.LoadSettingAsync<PayPalPayoutSettings>(storeScope);
            var vendorPayoutSettings = await _settingService.LoadSettingAsync<VendorPayoutSettings>(storeScope);
            paypalPayoutSettings.Active = model.Active;
            paypalPayoutSettings.ClientId = model.ClientId;
            paypalPayoutSettings.SecretKey = model.SecretKey;
            paypalPayoutSettings.UseSandbox = model.UseSandbox;

            await _settingService.SaveSettingOverridablePerStoreAsync(paypalPayoutSettings, x => x.Active, model.Active_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paypalPayoutSettings, x => x.ClientId, model.ClientId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paypalPayoutSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(paypalPayoutSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);

            var isPayPalActive = vendorPayoutSettings.ActivePayoutMethodSystemNames.Contains(PayPalDefaults.SystemName);
            if (model.Active && !isPayPalActive)
                vendorPayoutSettings.ActivePayoutMethodSystemNames.Add(PayPalDefaults.SystemName);

            if (!model.Active && isPayPalActive)
                vendorPayoutSettings.ActivePayoutMethodSystemNames.Remove(PayPalDefaults.SystemName);

            await _settingService.SaveSettingOverridablePerStoreAsync(vendorPayoutSettings, x => x.ActivePayoutMethodSystemNames, model.Active_OverrideForStore, storeScope, false);
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            return await Configure();
        }

        #region Vendor PayPal Configuration

        public async Task<IActionResult> ConfigureVendorPayPal(int vendorId)
        {
            if (!await _permissionService.AuthorizeAsync(PayPalPayoutPermissionProvider.ManagePayPalPayout))
                return AccessDeniedView();

            var vendorPayPalConfiguration = await _vendorPayPalConfigurationService.GetVendorPayPalConfigurationByVendorIdAsync(vendorId);
            if (vendorPayPalConfiguration != null)
            {
                var existingModel = new VendorPayPalConfigurationModel
                {
                    VendorId = vendorPayPalConfiguration.VendorId,
                    PayPalEmail = vendorPayPalConfiguration.PayPalEmail,
                };
                return View(existingModel);
            }
            var model = new VendorPayPalConfigurationModel
            {
                VendorId = vendorId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfigureVendorPayPal(VendorPayPalConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PayPalPayoutPermissionProvider.ManagePayPalPayout))
                return Content("Access denied");

            if (ModelState.IsValid)
            {
                var configurationToUpdate = await _vendorPayPalConfigurationService.GetVendorPayPalConfigurationByVendorIdAsync(model.VendorId);
                if (configurationToUpdate != null)
                {
                    configurationToUpdate.PayPalEmail = model.PayPalEmail;
                    await _vendorPayPalConfigurationService.UpdateVendorPaypalConfigutationAsync(configurationToUpdate);
                    return Ok();
                }
                var configurationToCreate = new VendorPayPalConfiguration
                {
                    VendorId = model.VendorId,
                    PayPalEmail = model.PayPalEmail,
                };
                await _vendorPayPalConfigurationService.InsertVendorPaypalConfigutationAsync(configurationToCreate);
                return Ok();
            }
            return Ok(new { Errors = GetErrorsFromModelState(ModelState) });
        }

        #endregion

        #region Utilities

        private IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
        {
            return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
        }

        #endregion

        #endregion
    }
}
