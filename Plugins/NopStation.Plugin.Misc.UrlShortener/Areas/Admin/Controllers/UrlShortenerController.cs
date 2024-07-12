using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models;
using NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Generate;
using NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Shortenurls;
using NopStation.Plugin.Misc.UrlShortener.Factories;

namespace NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Controllers
{
    public class UrlShortenerController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IShortenUrlModelFactory _shortenUrlModelFactory;

        #endregion

        #region Ctor

        public UrlShortenerController(ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreContext storeContext,
            ISettingService settingService,
            IPermissionService permissionService,
            IShortenUrlModelFactory shortenUrlModelFactory)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _settingService = settingService;
            _permissionService = permissionService;
            _shortenUrlModelFactory = shortenUrlModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(UrlShortnerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<UrlShortenerSettings>(storeScope);

            var model = new ConfigureModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                AccessToken = settings.AccessToken,
                EnableLog = settings.EnableLog
            };

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigureModel model)
        {
            if (!await _permissionService.AuthorizeAsync(UrlShortnerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<UrlShortenerSettings>(storeScope);

            settings.AccessToken = model.AccessToken;
            settings.EnableLog = model.EnableLog;

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AccessToken, model.AccessToken_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableLog, model.EnableLog_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.UpdateConfigure.Success"));

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(UrlShortnerPermissionProvider.ManageUrlShortner))
                return AccessDeniedView();

            var model = await _shortenUrlModelFactory.PrepareShortenUrlSearchModel(new ShortenUrlSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(ShortenUrlSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(UrlShortnerPermissionProvider.ManageUrlShortner))
                return await AccessDeniedDataTablesJson();

            var gridModel = await _shortenUrlModelFactory.PrepareShortenUrlListModel(model);
            return Json(gridModel);
        }

        [EditAccess, HttpPost, ActionName("List")]
        [FormValueRequired("generate-selected")]
        public async Task<IActionResult> GenerateShortUrl(GenerateShortUrlModel model)
        {
            if (!await _permissionService.AuthorizeAsync(UrlShortnerPermissionProvider.ManageUrlShortner))
                return AccessDeniedView();

            try
            {
                var response = await _shortenUrlModelFactory.GenerateShortUrls(model);
                if (response.Success > 0 && response.Fail <= 0)
                {
                    _notificationService.SuccessNotification(string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.GenerateShortenUrl.Success"), response.Success, response.Fail));
                }
                else if (response.Success > 0 && response.Fail > 0)
                {
                    _notificationService.WarningNotification(string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.GenerateShortenUrl.Warning"), response.Success, response.Fail));
                }
                else if (response.Success <= 0 && response.Fail > 0)
                {
                    _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.GenerateShortenUrl.Error"), response.Success, response.Fail) + " " + response.Message);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
            }

            return RedirectToAction(nameof(List));
        }

        [EditAccess, HttpPost, ActionName("List")]
        [FormValueRequired("generate-all")]
        public async Task<IActionResult> GenerateShortUrlAll(GenerateShortUrlModel model)
        {
            if (!await _permissionService.AuthorizeAsync(UrlShortnerPermissionProvider.ManageUrlShortner))
                return AccessDeniedView();

            try
            {
                var response = await _shortenUrlModelFactory.GenerateShortUrls(model, generateAll: true);

                if (response.Success > 0 && response.Fail <= 0)
                {
                    _notificationService.SuccessNotification(string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.GenerateShortenUrl.Success"), response.Success, response.Fail) + response.Message);
                }
                else if (response.Success > 0 && response.Fail > 0)
                {
                    _notificationService.WarningNotification(string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.GenerateShortenUrl.Warning"), response.Success, response.Fail) + response.Message);
                }
                else if (response.Success <= 0 && response.Fail > 0)
                {
                    var error = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(response.Message);
                    _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.GenerateShortenUrl.Error"), response.Success, response.Fail) + ". " + error);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
            }
            return RedirectToAction(nameof(List));
        }

        #endregion
    }
}
