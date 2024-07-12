using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.SMS.SmsTo.Areas.Admin.Factories;
using NopStation.Plugin.SMS.SmsTo.Areas.Admin.Models;
using NopStation.Plugin.SMS.SmsTo.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace NopStation.Plugin.SMS.SmsTo.Areas.Admin.Controllers
{
    public class QueuedSmsToSmsController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IQueuedSmsModelFactory _queuedSmsModelFactory;
        private readonly IQueuedSmsService _queuedSmsService;

        #endregion

        #region Ctor

        public QueuedSmsToSmsController(IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IQueuedSmsModelFactory queuedSmsModelFactory,
            IQueuedSmsService queuedSmsService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _queuedSmsModelFactory = queuedSmsModelFactory;
            _queuedSmsService = queuedSmsService;
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(SmsToPermissionProvider.ManageQueuedSms))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SmsToPermissionProvider.ManageQueuedSms))
                return AccessDeniedView();

            var searchModel = _queuedSmsModelFactory.PrepareQueuedSmsSearchModel(new QueuedSmsSearchModel());
            return View(searchModel);
        }

        public virtual async Task<IActionResult> GetList(QueuedSmsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SmsToPermissionProvider.ManageQueuedSms))
                return await AccessDeniedDataTablesJson();

            var model = await _queuedSmsModelFactory.PrepareQueuedSmsListModelAsync(searchModel);
            return Json(model);
        }
        
        public virtual async Task<IActionResult> View(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SmsToPermissionProvider.ManageQueuedSms))
                return AccessDeniedView();

            var queuedSms = await _queuedSmsService.GetQueuedSmsByIdAsync(id);
            if (queuedSms == null)
                return RedirectToAction("List");

            var model = await _queuedSmsModelFactory.PrepareQueuedSmsModelAsync(null, queuedSms);

            return View(model);
        }
        
        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SmsToPermissionProvider.ManageQueuedSms))
                return AccessDeniedView();

            var queuedSms = await _queuedSmsService.GetQueuedSmsByIdAsync(id);
            if (queuedSms == null)
                return RedirectToAction("List");

            await _queuedSmsService.DeleteQueuedSmsAsync(queuedSms);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SmsToSms.QueuedSmss.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
