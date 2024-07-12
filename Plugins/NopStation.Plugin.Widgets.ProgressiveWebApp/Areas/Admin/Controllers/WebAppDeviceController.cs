using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Controllers
{
    public class WebAppDeviceController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IWebAppDeviceService _webAppDeviceService;
        private readonly IWebAppDeviceModelFactory _webAppDeviceModelFactory;
        private readonly IPushNotificationSender _pushNotificationSender;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public WebAppDeviceController(IPermissionService permissionService,
            ILogger logger,
            IWorkContext workContext,
            IWebAppDeviceService webAppDeviceService,
            IWebAppDeviceModelFactory webAppDeviceModelFactory,
            IPushNotificationSender pushNotificationSender,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ProgressiveWebAppSettings progressiveWebAppSettings,
            INotificationService notificationService)
        {
            _permissionService = permissionService;
            _logger = logger;
            _workContext = workContext;
            _webAppDeviceService = webAppDeviceService;
            _webAppDeviceModelFactory = webAppDeviceModelFactory;
            _pushNotificationSender = pushNotificationSender;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _progressiveWebAppSettings = progressiveWebAppSettings;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageDevices))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageDevices))
                return AccessDeniedView();

            var searchModel = _webAppDeviceModelFactory.PrepareWebAppDeviceSearchModel(new WebAppDeviceSearchModel());
            return View(searchModel);
        }

        public virtual async Task<IActionResult> GetList(WebAppDeviceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageDevices))
                return await AccessDeniedDataTablesJson();

            var model = await _webAppDeviceModelFactory.PrepareWebAppDeviceListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> View(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageDevices))
                return AccessDeniedView();

            var webAppDevice = await _webAppDeviceService.GetWebAppDeviceByIdAsync(id);
            if (webAppDevice == null)
                return RedirectToAction("List");

            var model = await _webAppDeviceModelFactory.PrepareWebAppDeviceModelAsync(null, webAppDevice);

            return View(model);
        }

        [HttpPost, ActionName("View")]
        public virtual async Task<IActionResult> SendTest(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageDevices))
                return AccessDeniedView();

            var device = await _webAppDeviceService.GetWebAppDeviceByIdAsync(id);
            if (device == null)
                return RedirectToAction("List");

            try
            {
                _pushNotificationSender.SendNotification(device,
                    await _localizationService.GetResourceAsync("Admin.NopStation.PWA.WebAppDevices.TestPushTitle"),
                    await _localizationService.GetResourceAsync("Admin.NopStation.PWA.WebAppDevices.TestPushBody"),
                    (await _workContext.GetWorkingLanguageAsync()).Rtl ? "rtl" : "ltr",
                    await _pictureService.GetPictureUrlAsync(_progressiveWebAppSettings.DefaultIconId, 80));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PWA.WebAppDevices.TestPushSent"));
                return RedirectToAction("View", new { id = id });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"PushP256DH: {device.PushP256DH}{Environment.NewLine}" +
                            $"PushAuth : {device.PushAuth}{Environment.NewLine}CustomerId: {device.CustomerId}" +
                            $"{Environment.NewLine}" + ex.Message, ex);

                _notificationService.ErrorNotification(ex.Message);

                return RedirectToAction("List");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageDevices))
                return AccessDeniedView();

            var webAppDevice = await _webAppDeviceService.GetWebAppDeviceByIdAsync(id);
            if (webAppDevice == null)
                return RedirectToAction("List");

            await _webAppDeviceService.DeleteWebAppDeviceAsync(webAppDevice);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PWA.WebAppDevices.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                await _webAppDeviceService.DeleteWebAppDeviceAsync((await _webAppDeviceService.GetWebAppDevicesByIdsAsync(selectedIds.ToArray())));
            }

            return Json(new { Result = true });
        }

        #endregion
    }
}
