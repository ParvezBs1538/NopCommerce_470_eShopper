using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Controllers
{
    public class QueuedPushNotificationController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IQueuedPushNotificationModelFactory _queuedPushNotificationModelFactory;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;

        #endregion

        #region Ctor

        public QueuedPushNotificationController(IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IQueuedPushNotificationModelFactory queuedPushNotificationModelFactory,
            IQueuedPushNotificationService queuedPushNotificationService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _queuedPushNotificationModelFactory = queuedPushNotificationModelFactory;
            _queuedPushNotificationService = queuedPushNotificationService;
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            var searchModel = _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationSearchModel(new QueuedPushNotificationSearchModel());
            return View(searchModel);
        }

        public virtual async Task<IActionResult> GetList(QueuedPushNotificationSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageQueuedNotifications))
                return await AccessDeniedDataTablesJson();

            var model = await _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> View(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            var queuedPushNotification = await _queuedPushNotificationService.GetQueuedPushNotificationByIdAsync(id);
            if (queuedPushNotification == null)
                return RedirectToAction("List");

            var model = await _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationModelAsync(null, queuedPushNotification);

            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProgressiveWebAppPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            var queuedPushNotification = await _queuedPushNotificationService.GetQueuedPushNotificationByIdAsync(id);
            if (queuedPushNotification == null)
                return RedirectToAction("List");

            await _queuedPushNotificationService.DeleteQueuedPushNotificationAsync(queuedPushNotification);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PWA.QueuedPushNotifications.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
