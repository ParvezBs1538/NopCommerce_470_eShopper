using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.AmazonS3.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.AmazonS3.Controllers
{
    public partial class AmazonS3Controller : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;

        #endregion

        public AmazonS3Controller(IStoreContext storeContext,
            IPermissionService permissionService,
            ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService)
        {
            _storeContext = storeContext;
            _permissionService = permissionService;
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AmazonS3PermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var amazonS3Settings = await _settingService.LoadSettingAsync<AmazonS3Settings>(storeScope);

            var model = new ConfigurationModel
            {
                AWSS3Enable = amazonS3Settings.AWSS3Enable,
                RegionEndpointId = amazonS3Settings.RegionEndpointId,
                AWSS3AccessKeyId = amazonS3Settings.AWSS3AccessKeyId,
                AWSS3SecretKey = amazonS3Settings.AWSS3SecretKey,
                AWSS3BucketName = amazonS3Settings.AWSS3BucketName,
                AWSS3RootUrl = amazonS3Settings.AWSS3RootUrl,
                ExpiresDays = amazonS3Settings.ExpiresDays,
                EnableCdn = amazonS3Settings.EnableCdn,
                CdnBaseUrl = amazonS3Settings.CdnBaseUrl,
                CannedACLId = amazonS3Settings.CannedACLId,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.AWSS3Enable_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.AWSS3Enable, storeScope);
                model.RegionEndpointId_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.RegionEndpointId, storeScope);
                model.AWSS3AccessKeyId_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.AWSS3AccessKeyId, storeScope);
                model.AWSS3SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.AWSS3SecretKey, storeScope);
                model.AWSS3BucketName_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.AWSS3BucketName, storeScope);
                model.AWSS3RootUrl_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.AWSS3RootUrl, storeScope);
                model.ExpiresDays_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.ExpiresDays, storeScope);
                model.EnableCdn_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.EnableCdn, storeScope);
                model.CdnBaseUrl_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.CdnBaseUrl, storeScope);
                model.CannedACLId_OverrideForStore = await _settingService.SettingExistsAsync(amazonS3Settings, x => x.CannedACLId, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Misc.AmazonS3/Views/AmazonS3/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AmazonS3PermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var amazonS3Settings = await _settingService.LoadSettingAsync<AmazonS3Settings>(storeScope);

            //get previous picture identifiers
            amazonS3Settings.AWSS3Enable = model.AWSS3Enable;
            amazonS3Settings.RegionEndpointId = model.RegionEndpointId;
            amazonS3Settings.AWSS3AccessKeyId = model.AWSS3AccessKeyId;
            amazonS3Settings.AWSS3SecretKey = model.AWSS3SecretKey;
            amazonS3Settings.AWSS3BucketName = model.AWSS3BucketName;
            amazonS3Settings.AWSS3RootUrl = model.AWSS3RootUrl;
            amazonS3Settings.ExpiresDays = model.ExpiresDays;
            amazonS3Settings.EnableCdn = model.EnableCdn;
            amazonS3Settings.CdnBaseUrl = model.CdnBaseUrl;
            amazonS3Settings.CannedACLId = model.CannedACLId;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.AWSS3Enable, model.AWSS3Enable_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.RegionEndpointId, model.RegionEndpointId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.AWSS3AccessKeyId, model.AWSS3AccessKeyId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.AWSS3SecretKey, model.AWSS3SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.AWSS3BucketName, model.AWSS3BucketName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.AWSS3RootUrl, model.AWSS3RootUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.ExpiresDays, model.ExpiresDays_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.EnableCdn, model.EnableCdn_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.CdnBaseUrl, model.CdnBaseUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonS3Settings, x => x.CannedACLId, model.CannedACLId_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }
    }
}
