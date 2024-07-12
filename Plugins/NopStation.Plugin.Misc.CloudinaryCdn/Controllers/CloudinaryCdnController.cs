using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.CloudinaryCdn.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.CloudinaryCdn.Controllers
{
    public class CloudinaryCdnController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly CloudinaryCdnSettings _cloudinaryCdnSettings;

        #endregion

        #region Ctor

        public CloudinaryCdnController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            CloudinaryCdnSettings cloudinaryCdnSettings)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _cloudinaryCdnSettings = cloudinaryCdnSettings;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(CloudinaryCdnPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = new ConfigurationModel()
            {
                ApiKey = _cloudinaryCdnSettings.ApiKey,
                ApiSecret = _cloudinaryCdnSettings.ApiSecret,
                CdnFolderName = _cloudinaryCdnSettings.CdnFolderName,
                CloudName = _cloudinaryCdnSettings.CloudName,
                EnableCssCdn = _cloudinaryCdnSettings.EnableCssCdn,
                Enabled = _cloudinaryCdnSettings.Enabled,
                EnableJsCdn = _cloudinaryCdnSettings.EnableJsCdn,
                PrependCdnFolderName = _cloudinaryCdnSettings.PrependCdnFolderName,
                AutoGenerateAssociatedProductPicture = _cloudinaryCdnSettings.AutoGenerateAssociatedProductPicture,
                AutoGenerateAutoCompleteSearchThumbPicture = _cloudinaryCdnSettings.AutoGenerateAutoCompleteSearchThumbPicture,
                AutoGenerateCartThumbPicture = _cloudinaryCdnSettings.AutoGenerateCartThumbPicture,
                AutoGenerateCategoryThumbPicture = _cloudinaryCdnSettings.AutoGenerateCategoryThumbPicture,
                AutoGenerateManufacturerThumbPicture = _cloudinaryCdnSettings.AutoGenerateManufacturerThumbPicture,
                AutoGenerateMiniCartThumbPicture = _cloudinaryCdnSettings.AutoGenerateMiniCartThumbPicture,
                AutoGenerateProductDetailsPicture = _cloudinaryCdnSettings.AutoGenerateProductDetailsPicture,
                AutoGenerateProductThumbPicture = _cloudinaryCdnSettings.AutoGenerateProductThumbPicture,
                AutoGenerateProductThumbPictureOnProductDetailsPage = _cloudinaryCdnSettings.AutoGenerateProductThumbPictureOnProductDetailsPage,
                AutoGenerateRequiredPictures = _cloudinaryCdnSettings.AutoGenerateRequiredPictures,
                AutoGenerateVendorThumbPicture = _cloudinaryCdnSettings.AutoGenerateVendorThumbPicture
            };

            return View("~/Plugins/NopStation.Plugin.Misc.CloudinaryCdn/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(CloudinaryCdnPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            _cloudinaryCdnSettings.ApiKey = model.ApiKey;
            _cloudinaryCdnSettings.ApiSecret = model.ApiSecret;
            _cloudinaryCdnSettings.CdnFolderName = model.CdnFolderName;
            _cloudinaryCdnSettings.CloudName = model.CloudName;
            _cloudinaryCdnSettings.EnableCssCdn = model.EnableCssCdn;
            _cloudinaryCdnSettings.Enabled = model.Enabled;
            _cloudinaryCdnSettings.EnableJsCdn = model.EnableJsCdn;
            _cloudinaryCdnSettings.PrependCdnFolderName = model.PrependCdnFolderName;
            _cloudinaryCdnSettings.AutoGenerateAssociatedProductPicture = model.AutoGenerateAssociatedProductPicture;
            _cloudinaryCdnSettings.AutoGenerateAutoCompleteSearchThumbPicture = model.AutoGenerateAutoCompleteSearchThumbPicture;
            _cloudinaryCdnSettings.AutoGenerateCartThumbPicture = model.AutoGenerateCartThumbPicture;
            _cloudinaryCdnSettings.AutoGenerateCategoryThumbPicture = model.AutoGenerateCategoryThumbPicture;
            _cloudinaryCdnSettings.AutoGenerateManufacturerThumbPicture = model.AutoGenerateManufacturerThumbPicture;
            _cloudinaryCdnSettings.AutoGenerateMiniCartThumbPicture = model.AutoGenerateMiniCartThumbPicture;
            _cloudinaryCdnSettings.AutoGenerateProductDetailsPicture = model.AutoGenerateProductDetailsPicture;
            _cloudinaryCdnSettings.AutoGenerateProductThumbPicture = model.AutoGenerateProductThumbPicture;
            _cloudinaryCdnSettings.AutoGenerateProductThumbPictureOnProductDetailsPage = model.AutoGenerateProductThumbPictureOnProductDetailsPage;
            _cloudinaryCdnSettings.AutoGenerateRequiredPictures = model.AutoGenerateRequiredPictures;
            _cloudinaryCdnSettings.AutoGenerateVendorThumbPicture = model.AutoGenerateVendorThumbPicture;

            await _settingService.SaveSettingAsync(_cloudinaryCdnSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion
    }
}
