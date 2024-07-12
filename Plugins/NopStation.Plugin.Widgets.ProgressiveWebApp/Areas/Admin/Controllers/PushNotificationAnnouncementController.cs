using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Controllers
{
    public class PushNotificationAnnouncementController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPushNotificationAnnouncementModelFactory _pushNotificationAnnouncementModelFactory;
        private readonly IPushNotificationAnnouncementService _pushNotificationAnnouncementService;
        private readonly IWorkflowNotificationService _workflowNotificationService;
        private readonly IPictureService _pictureService;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PushNotificationAnnouncementController(IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPushNotificationAnnouncementModelFactory pushNotificationAnnouncementModelFactory,
            IPushNotificationAnnouncementService pushNotificationAnnouncementService,
            IWorkflowNotificationService workflowNotificationService,
            IPictureService pictureService,
            ProgressiveWebAppSettings progressiveWebAppSettings,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _pushNotificationAnnouncementModelFactory = pushNotificationAnnouncementModelFactory;
            _pushNotificationAnnouncementService = pushNotificationAnnouncementService;
            _workflowNotificationService = workflowNotificationService;
            _pictureService = pictureService;
            _progressiveWebAppSettings = progressiveWebAppSettings;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
                return AccessDeniedView();

            var searchModel = _pushNotificationAnnouncementModelFactory.PreparePushNotificationAnnouncementSearchModel(new PushNotificationAnnouncementSearchModel());
            return View(searchModel);
        }

        public virtual async Task<IActionResult> GetList(PushNotificationAnnouncementSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
                return await AccessDeniedDataTablesJson();

            var model = await _pushNotificationAnnouncementModelFactory.PreparePushNotificationAnnouncementListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
                return AccessDeniedView();

            var model = await _pushNotificationAnnouncementModelFactory.PreparePushNotificationAnnouncementModelAsync(new PushNotificationAnnouncementModel(), null);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(PushNotificationAnnouncementModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var pushNotificationAnnouncement = model.ToEntity<PushNotificationAnnouncement>();
                pushNotificationAnnouncement.CreatedOnUtc = DateTime.UtcNow;

                await _pushNotificationAnnouncementService.InsertPushNotificationAnnouncementAsync(pushNotificationAnnouncement);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PWA.PushNotificationAnnouncements.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = pushNotificationAnnouncement.Id });
            }

            model = await _pushNotificationAnnouncementModelFactory.PreparePushNotificationAnnouncementModelAsync(model, null);
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
                return AccessDeniedView();

            var pushNotificationAnnouncement = await _pushNotificationAnnouncementService.GetPushNotificationAnnouncementByIdAsync(id);
            if (pushNotificationAnnouncement == null)
                return RedirectToAction("List");

            var model = await _pushNotificationAnnouncementModelFactory.PreparePushNotificationAnnouncementModelAsync(null, pushNotificationAnnouncement);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(PushNotificationAnnouncementModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
                return AccessDeniedView();

            var pushNotificationAnnouncement = await _pushNotificationAnnouncementService.GetPushNotificationAnnouncementByIdAsync(model.Id);
            if (pushNotificationAnnouncement == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                pushNotificationAnnouncement = model.ToEntity(pushNotificationAnnouncement);
                await _pushNotificationAnnouncementService.UpdatePushNotificationAnnouncementAsync(pushNotificationAnnouncement);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PWA.PushNotificationAnnouncements.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = pushNotificationAnnouncement.Id });
            }
            model = await _pushNotificationAnnouncementModelFactory.PreparePushNotificationAnnouncementModelAsync(model, pushNotificationAnnouncement);
            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
                return AccessDeniedView();

            var pushNotificationAnnouncement = await _pushNotificationAnnouncementService.GetPushNotificationAnnouncementByIdAsync(id);
            if (pushNotificationAnnouncement == null)
                return RedirectToAction("List");

            await _pushNotificationAnnouncementService.DeletePushNotificationAnnouncementAsync(pushNotificationAnnouncement);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PWA.PushNotificationAnnouncements.Deleted"));

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> SendNow(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageAnnouncements))
                return AccessDeniedView();

            var announcement = await _pushNotificationAnnouncementService.GetPushNotificationAnnouncementByIdAsync(id);
            if (announcement == null)
                return RedirectToAction("List");

            var iconUrl = "";
            if (announcement.UseDefaultIcon)
                iconUrl = await _pictureService.GetPictureUrlAsync(_progressiveWebAppSettings.DefaultIconId, 80);
            else
                iconUrl = await _pictureService.GetPictureUrlAsync(announcement.IconId, 80);

            var imageUrl = await _pictureService.GetPictureUrlAsync(announcement.ImageId, showDefaultPicture: false);

            _workflowNotificationService.SendNotification(announcement.Title, announcement.Body, iconUrl,
                imageUrl, announcement.Url, (await _storeContext.GetCurrentStoreAsync()).Id, 0, (await _workContext.GetWorkingLanguageAsync()).Rtl);

            return RedirectToAction("Edit", new { id = announcement.Id });
        }

        #endregion
    }
}
