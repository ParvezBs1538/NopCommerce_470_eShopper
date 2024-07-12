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
    public class HelpdeskDepartmentController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IDepartmentModelFactory _helpdeskDepartmentModelFactory;
        private readonly IDepartmentService _helpdeskDepartmentService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public HelpdeskDepartmentController(ILocalizationService localizationService,
            INotificationService notificationService,
            IDepartmentModelFactory helpdeskDepartmentModelFactory,
            IDepartmentService helpdeskDepartmentService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _helpdeskDepartmentModelFactory = helpdeskDepartmentModelFactory;
            _helpdeskDepartmentService = helpdeskDepartmentService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageDepartments))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageDepartments))
                return AccessDeniedView();

            var searchModel = _helpdeskDepartmentModelFactory.PrepareDepartmentSearchModel(new DepartmentSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(DepartmentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageDepartments))
                return await AccessDeniedDataTablesJson();

            var model = await _helpdeskDepartmentModelFactory.PrepareDepartmentListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageDepartments))
                return AccessDeniedView();

            var model = _helpdeskDepartmentModelFactory.PrepareDepartmentModel(new DepartmentModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(DepartmentModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageDepartments))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var helpdeskDepartment = model.ToEntity<HelpdeskDepartment>();
                await _helpdeskDepartmentService.InsertHelpdeskDepartmentAsync(helpdeskDepartment);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Departments.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = helpdeskDepartment.Id });
            }

            model = _helpdeskDepartmentModelFactory.PrepareDepartmentModel(new DepartmentModel(), null);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageDepartments))
                return AccessDeniedView();

            var helpdeskDepartment = await _helpdeskDepartmentService.GetHelpdeskDepartmentByIdAsync(id);
            if (helpdeskDepartment == null)
                return RedirectToAction("List");

            var model = _helpdeskDepartmentModelFactory.PrepareDepartmentModel(null, helpdeskDepartment);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(DepartmentModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageDepartments))
                return AccessDeniedView();

            var helpdeskDepartment = await _helpdeskDepartmentService.GetHelpdeskDepartmentByIdAsync(model.Id);
            if (helpdeskDepartment == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                helpdeskDepartment = model.ToEntity(helpdeskDepartment);
                await _helpdeskDepartmentService.UpdateHelpdeskDepartmentAsync(helpdeskDepartment);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Departments.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = helpdeskDepartment.Id });
            }
            model = _helpdeskDepartmentModelFactory.PrepareDepartmentModel(model, helpdeskDepartment);
            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageDepartments))
                return AccessDeniedView();

            var helpdeskDepartment = await _helpdeskDepartmentService.GetHelpdeskDepartmentByIdAsync(id);
            if (helpdeskDepartment == null)
                return RedirectToAction("List");

            await _helpdeskDepartmentService.DeleteHelpdeskDepartmentAsync(helpdeskDepartment);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Departments.Deleted"));

            return RedirectToAction("List");
        }

        #endregion
    }
}
