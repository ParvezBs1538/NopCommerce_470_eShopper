using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Controllers
{
    public class WebApiQueuedNotificationController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IQueuedPushNotificationModelFactory _queuedPushNotificationModelFactory;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public WebApiQueuedNotificationController(ILocalizationService localizationService,
            INotificationService notificationService,
            IQueuedPushNotificationModelFactory queuedPushNotificationModelFactory,
            IQueuedPushNotificationService queuedPushNotificationService,
            IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _queuedPushNotificationModelFactory = queuedPushNotificationModelFactory;
            _queuedPushNotificationService = queuedPushNotificationService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            var searchModel = _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationSearchModel(new WebApiQueuedNotificationSearchModel());
            return View(searchModel);
        }

        public virtual async Task<IActionResult> GetList(WebApiQueuedNotificationSearchModel searchModel)
        {
            var model = await _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> View(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageQueuedNotifications))
                return await AccessDeniedDataTablesJson();

            var queuedPushNotification = await _queuedPushNotificationService.GetQueuedPushNotificationByIdAsync(id);
            if (queuedPushNotification == null)
                return RedirectToAction("List");

            var model = await _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationModelAsync(null, queuedPushNotification);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            //try to get a queued queuedPushNotification with the specified id
            var queuedPushNotification = await _queuedPushNotificationService.GetQueuedPushNotificationByIdAsync(id);
            if (queuedPushNotification == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationModelAsync(null, queuedPushNotification);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Edit(WebApiQueuedNotificationModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            var queuedPushNotification = await _queuedPushNotificationService.GetQueuedPushNotificationByIdAsync(model.Id);
            if (queuedPushNotification == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                queuedPushNotification = model.ToEntity(queuedPushNotification);
                queuedPushNotification.DontSendBeforeDateUtc = model.SendImmediately || !model.DontSendBeforeDate.HasValue ?
                    null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DontSendBeforeDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
                await _queuedPushNotificationService.UpdateQueuedPushNotificationAsync(queuedPushNotification);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.System.QueuedEmails.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = queuedPushNotification.Id }) : RedirectToAction("List");
            }

            model = await _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationModelAsync(model, queuedPushNotification, true);

            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            var queuedPushNotification = await _queuedPushNotificationService.GetQueuedPushNotificationByIdAsync(id);
            if (queuedPushNotification == null)
                return RedirectToAction("List");

            await _queuedPushNotificationService.DeleteQueuedPushNotificationAsync(queuedPushNotification);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Deleted"));

            return RedirectToAction("List");
        }
        public virtual async Task<IActionResult> DeleteSentQueuedPushNotification()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageQueuedNotifications))
                return AccessDeniedView();

            await _queuedPushNotificationService.DeleteSentQueuedPushNotificationAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
