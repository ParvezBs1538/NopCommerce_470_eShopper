using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure;
using WebPush;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Controllers
{
    public class ProgressiveWebAppController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IWebAppModelFactory _webAppModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly INopFileProvider _fileProvider;
        private readonly IPictureService _pictureService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public ProgressiveWebAppController(IPermissionService permissionService,
            INotificationService notificationService,
            IWebAppModelFactory webAppModelFactory,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            INopFileProvider fileProvider,
            IPictureService pictureService,
            IStoreService storeService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _webAppModelFactory = webAppModelFactory;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _fileProvider = fileProvider;
            _pictureService = pictureService;
            _storeService = storeService;
        }

        #endregion

        #region Utilities

        protected async Task PreparePWAStaticFileAsync(int storeId)
        {
            if (storeId == 0)
            {
                var stores = await _storeService.GetAllStoresAsync();
                foreach (var store in stores)
                {
                    await PrepareManifestStringAsync(store.Id);
                }
            }
            else
                await PrepareManifestStringAsync(storeId);
        }

        protected async Task PrepareManifestStringAsync(int storeId)
        {
            var pwaSettings = await _settingService.LoadSettingAsync<ProgressiveWebAppSettings>(storeId);
            var manifestPath = string.Format(PWADefaults.ManifestUrl, storeId.ToString("000000"));
            var manifestFilePath = _fileProvider.GetAbsolutePath(manifestPath);
            _fileProvider.CreateFile(manifestFilePath);

            var model = new ManifestModel()
            {
                BackgroundColor = pwaSettings.ManifestBackgroundColor,
                Display = pwaSettings.ManifestDisplay,
                Name = pwaSettings.ManifestName,
                ShortName = pwaSettings.ManifestShortName,
                StartUrl = pwaSettings.ManifestStartUrl,
                ThemeColor = pwaSettings.ManifestThemeColor,
                SplashPages = null,
                ApplicationScope = pwaSettings.ManifestAppScope
            };

            var picture = await _pictureService.GetPictureByIdAsync(pwaSettings.ManifestPictureId);
            if (picture != null)
            {
                model.Icons.Add(new ManifestModel.ManifestIconModel()
                {
                    Source = await _pictureService.GetPictureUrlAsync(picture.Id, 72),
                    Sizes = "72x72",
                    Type = picture.MimeType
                });
                model.Icons.Add(new ManifestModel.ManifestIconModel()
                {
                    Source = await _pictureService.GetPictureUrlAsync(picture.Id, 96),
                    Sizes = "96x96",
                    Type = picture.MimeType
                });
                model.Icons.Add(new ManifestModel.ManifestIconModel()
                {
                    Source = await _pictureService.GetPictureUrlAsync(picture.Id, 128),
                    Sizes = "128x128",
                    Type = picture.MimeType
                });
                model.Icons.Add(new ManifestModel.ManifestIconModel()
                {
                    Source = await _pictureService.GetPictureUrlAsync(picture.Id, 144),
                    Sizes = "144x144",
                    Type = picture.MimeType
                });
                model.Icons.Add(new ManifestModel.ManifestIconModel()
                {
                    Source = await _pictureService.GetPictureUrlAsync(picture.Id, 192),
                    Sizes = "192x192",
                    Type = picture.MimeType
                });
                model.Icons.Add(new ManifestModel.ManifestIconModel()
                {
                    Source = await _pictureService.GetPictureUrlAsync(picture.Id, 384),
                    Sizes = "384x384",
                    Type = picture.MimeType
                });
                model.Icons.Add(new ManifestModel.ManifestIconModel()
                {
                    Source = await _pictureService.GetPictureUrlAsync(picture.Id, 512),
                    Sizes = "512x512",
                    Type = picture.MimeType
                });
            }

            var json = JsonConvert.SerializeObject(model);
            if (string.IsNullOrWhiteSpace(pwaSettings.ManifestAppScope))
            {
                var o = (JObject)JsonConvert.DeserializeObject(json);
                o.Property("scope").Remove();
                json = o.ToString();
            }


            var obj = JsonConvert.DeserializeObject<dynamic>(json);
            obj.icons[model.Icons.Count - 1].purpose = "any maskable";
            json = JsonConvert.SerializeObject(obj);

            _fileProvider.WriteAllText(manifestFilePath, json, Encoding.UTF8);
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _webAppModelFactory.PrepareConfigurationModelAsync();
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var webAppSettings = await _settingService.LoadSettingAsync<ProgressiveWebAppSettings>(storeScope);
            webAppSettings = model.ToSettings(webAppSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.VapidSubjectEmail, model.VapidSubjectEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.VapidPublicKey, model.VapidPublicKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.VapidPrivateKey, model.VapidPrivateKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.DisableSilent, model.DisableSilent_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.Vibration, model.Vibration_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.SoundFileUrl, model.SoundFileUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.DefaultIconId, model.DefaultIconId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.ManifestName, model.ManifestName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.ManifestShortName, model.ManifestShortName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.ManifestThemeColor, model.ManifestThemeColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.ManifestBackgroundColor, model.ManifestBackgroundColor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.ManifestDisplay, model.ManifestDisplay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.ManifestStartUrl, model.ManifestStartUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.ManifestAppScope, model.ManifestAppScope_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.ManifestPictureId, model.ManifestPictureId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.AbandonedCartCheckingOffset, model.AbandonedCartCheckingOffset_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(webAppSettings, x => x.UnitTypeId, model.UnitTypeId_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            await PreparePWAStaticFileAsync(storeScope);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> GetVapidKeys()
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var keys = VapidHelper.GenerateVapidKeys();
            return Json(keys);
        }

        #endregion
    }
}
