using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Helpdesk.Domains;
using NopStation.Plugin.Widgets.Helpdesk.Services;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Controllers
{
    public class HelpdeskStaffController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStaffModelFactory _helpdeskStaffModelFactory;
        private readonly IStaffService _helpdeskStaffService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public HelpdeskStaffController(ILocalizationService localizationService,
            INotificationService notificationService,
            IStaffModelFactory helpdeskStaffModelFactory,
            IStaffService helpdeskStaffService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _helpdeskStaffModelFactory = helpdeskStaffModelFactory;
            _helpdeskStaffService = helpdeskStaffService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageStaffs))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageStaffs))
                return AccessDeniedView();

            var searchModel = _helpdeskStaffModelFactory.PrepareStaffSearchModel(new StaffSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(StaffSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageStaffs))
                return await AccessDeniedDataTablesJson();

            var model = await _helpdeskStaffModelFactory.PrepareStaffListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageStaffs))
                return AccessDeniedView();

            var model = _helpdeskStaffModelFactory.PrepareStaffModel(new StaffModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(StaffModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageStaffs))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var helpdeskStaff = model.ToEntity<HelpdeskStaff>();
                await _helpdeskStaffService.InsertHelpdeskStaffAsync(helpdeskStaff);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Staffs.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = helpdeskStaff.Id });
            }

            model = _helpdeskStaffModelFactory.PrepareStaffModel(model, null);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageStaffs))
                return AccessDeniedView();

            var helpdeskStaff = await _helpdeskStaffService.GetHelpdeskStaffByIdAsync(id);
            if (helpdeskStaff == null)
                return RedirectToAction("List");

            var model = _helpdeskStaffModelFactory.PrepareStaffModel(null, helpdeskStaff);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(StaffModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageStaffs))
                return AccessDeniedView();

            var helpdeskStaff = await _helpdeskStaffService.GetHelpdeskStaffByIdAsync(model.Id);
            if (helpdeskStaff == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                helpdeskStaff = model.ToEntity(helpdeskStaff);
                await _helpdeskStaffService.UpdateHelpdeskStaffAsync(helpdeskStaff);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Staffs.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = helpdeskStaff.Id });
            }
            model = _helpdeskStaffModelFactory.PrepareStaffModel(model, helpdeskStaff);
            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageStaffs))
                return AccessDeniedView();

            var helpdeskStaff = await _helpdeskStaffService.GetHelpdeskStaffByIdAsync(id);
            if (helpdeskStaff == null)
                return RedirectToAction("List");

            await _helpdeskStaffService.DeleteHelpdeskStaffAsync(helpdeskStaff);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Staffs.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
