using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Extensions;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Controllers
{
    public class DMSController : NopStationAdminController
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        public DMSController(IStoreContext storeContext,
            ISettingService settingService,
            IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        private string GenerateNST(DMSSettings settings)
        {
            if (!settings.EnableJwtSecurity || string.IsNullOrWhiteSpace(settings.TokenSecret))
                return null;

            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object>()
                {
                    { "NST-KEY", settings.TokenKey },
                    { "iat", now }
                };

            return JwtHelper.JwtEncoder.Encode(payload, settings.TokenSecret);
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<DMSSettings>(storeScope);

            var model = settings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;

            model.NST = GenerateNST(settings);

            #region Select Lists

            model.AvailablePaperSizes = (await PaperSizeEnum.A4.ToSelectListAsync(false))
                    .Select(item => new SelectListItem(item.Text, item.Value))
                    .ToList();

            model.AvailableProofOfDeliveryType = (await ProofOfDeliveryTypes.CustomersSignature.ToSelectListAsync(false))
                    .Select(item => new SelectListItem(item.Text, item.Value))
                    .ToList();

            for (int i = 15; i <= 300; i += 15)
            {
                model.AvailableLocationUpdateIntervals.Add(new SelectListItem(i.ToString("000"), i.ToString("000")));
            }

            #endregion

            if (storeScope == 0)
                return View(model);

            #region Common
            model.AllowShippersToSelectPageSize_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AllowShippersToSelectPageSize, storeScope);
            model.PageSizeOptions_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PageSizeOptions, storeScope);
            model.ShipmentPageSize_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShipmentPageSize, storeScope);
            model.UseAjaxLoading_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UseAjaxLoading, storeScope);
            //model.EnableSignatureUpload_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableSignatureUpload, storeScope);
            model.GeoMapId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.GeoMapId, storeScope);
            model.GoogleMapApiKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.GoogleMapApiKey, storeScope);
            model.LocationUpdateIntervalInSeconds_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.LocationUpdateIntervalInSeconds, storeScope);
            model.AllowCustomersToDeleteAccount_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AllowCustomersToDeleteAccount, storeScope);

            #endregion

            #region Security

            //model.SignatureUploadRequired_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.SignatureUploadRequired, storeScope);
            model.CheckIat_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CheckIat, storeScope);
            model.TokenKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TokenKey, storeScope);
            model.TokenSecondsValid_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TokenSecondsValid, storeScope);
            model.TokenSecret_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TokenSecret, storeScope);
            model.EnableJwtSecurity_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableJwtSecurity, storeScope);

            #endregion

            #region PackagingSlip

            model.DefaultPackagingSlipPaperSizeId_OverrideForStore = false;
            model.PrintPackagingSlipLandscape_OverrideForStore = false;
            model.PrintProductsOnPackagingSlip_OverrideForStore = false;
            model.PrintWeightInfoOnPackagingSlip_OverrideForStore = false;
            model.PrintEachPackagingSlipInNewPage_OverrideForStore = false;

            #endregion

            #region ProofOfDelivery

            model.EnabledProofOfDelivery_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnabledProofOfDelivery, storeScope);
            model.ProofOfDeliveryTypeId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ProofOfDeliveryTypeId, storeScope);
            model.ProofOfDeliveryRequired_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ProofOfDeliveryRequired, storeScope);
            model.ProofOfDeliveryImageMaxSize_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ProofOfDeliveryImageMaxSize, storeScope);
            model.OtpLength_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.OtpLength, storeScope);
            //model.TwilioAccountSid_OverrideForStore = false;
            //model.TwilioAuthToken_OverrideForStore = false;
            //model.TwilioPhoneNumber_OverrideForStore = false;

            #endregion


            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<DMSSettings>(storeScope);
            settings = model.ToSettings(settings);

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowShippersToSelectPageSize, model.AllowShippersToSelectPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.GeoMapId, model.GeoMapId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.GoogleMapApiKey, model.GoogleMapApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PageSizeOptions, model.PageSizeOptions_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShipmentPageSize, model.ShipmentPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UseAjaxLoading, model.UseAjaxLoading_OverrideForStore, storeScope, false);
            //await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableSignatureUpload, model.EnableSignatureUpload_OverrideForStore, storeScope, false);
            //await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SignatureUploadRequired, model.SignatureUploadRequired_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CheckIat, model.CheckIat_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TokenKey, model.TokenKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TokenSecondsValid, model.TokenSecondsValid_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TokenSecret, model.TokenSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableJwtSecurity, model.EnableJwtSecurity_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.LocationUpdateIntervalInSeconds, model.LocationUpdateIntervalInSeconds_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowCustomersToDeleteAccount, model.AllowCustomersToDeleteAccount_OverrideForStore, storeScope, false);

            #region PackagingSlip

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultPackagingSlipPaperSizeId, false, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PrintPackagingSlipLandscape, false, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PrintProductsOnPackagingSlip, false, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PrintWeightInfoOnPackagingSlip, false, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PrintEachPackagingSlipInNewPage, false, storeScope, false);

            #endregion

            #region ProofOfDelivery

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnabledProofOfDelivery, model.EnabledProofOfDelivery_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ProofOfDeliveryTypeId, model.ProofOfDeliveryTypeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ProofOfDeliveryRequired, model.ProofOfDeliveryRequired_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ProofOfDeliveryImageMaxSize, model.ProofOfDeliveryImageMaxSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OtpLength, model.OtpLength_OverrideForStore, storeScope, false);
            //await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TwilioAccountSid, false, storeScope, false);
            //await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TwilioAuthToken, false, storeScope, false);
            //await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TwilioPhoneNumber, false, storeScope, false);

            #endregion

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
