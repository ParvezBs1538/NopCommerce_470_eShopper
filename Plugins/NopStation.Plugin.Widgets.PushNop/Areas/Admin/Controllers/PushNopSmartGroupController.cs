using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PushNop.Domains;
using NopStation.Plugin.Widgets.PushNop.Services;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Controllers
{
    public class PushNopSmartGroupController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISmartGroupModelFactory _smartGroupModelFactory;
        private readonly ISmartGroupService _smartGroupService;
        private readonly IPermissionService _permissionService;
        private readonly ISmartGroupNotificationService _smartGroupNotificationService;

        #endregion

        #region Ctor

        public PushNopSmartGroupController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISmartGroupModelFactory smartGroupModelFactory,
            ISmartGroupService smartGroupService,
            IPermissionService permissionService,
            ISmartGroupNotificationService smartGroupNotificationService)
        {
            _permissionService = permissionService;
            _smartGroupNotificationService = smartGroupNotificationService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _smartGroupModelFactory = smartGroupModelFactory;
            _smartGroupService = smartGroupService;
        }

        #endregion

        #region Smart groups        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var searchModel = await _smartGroupModelFactory.PrepareSmartGroupSearchModelAsync(new SmartGroupSearchModel());
            return View(searchModel);
        }

        public virtual async Task<IActionResult> GetList(SmartGroupSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return await AccessDeniedDataTablesJson();

            var model = await _smartGroupModelFactory.PrepareSmartGroupListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var model = await _smartGroupModelFactory.PrepareSmartGroupModelAsync(new SmartGroupModel(), null);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(SmartGroupModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var smartGroup = model.ToEntity<SmartGroup>();
                smartGroup.CreatedOnUtc = DateTime.UtcNow;

                await _smartGroupService.InsertSmartGroupAsync(smartGroup);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.SmartGroups.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = smartGroup.Id });
            }

            model = await _smartGroupModelFactory.PrepareSmartGroupModelAsync(model, null);
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var smartGroup = await _smartGroupService.GetSmartGroupByIdAsync(id);
            if (smartGroup == null)
                return RedirectToAction("List");

            var model = await _smartGroupModelFactory.PrepareSmartGroupModelAsync(null, smartGroup);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(SmartGroupModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var smartGroup = await _smartGroupService.GetSmartGroupByIdAsync(model.Id);
            if (smartGroup == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                smartGroup = model.ToEntity(smartGroup);
                await _smartGroupService.UpdateSmartGroupAsync(smartGroup);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.SmartGroups.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = smartGroup.Id });
            }
            model = await _smartGroupModelFactory.PrepareSmartGroupModelAsync(model, smartGroup);
            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var smartGroup = await _smartGroupService.GetSmartGroupByIdAsync(id);
            if (smartGroup == null)
                return RedirectToAction("List");

            var campaigns = await _smartGroupNotificationService.GetSmartGroupNotificationsBySmartGroupIdAsync(smartGroup.Id);
            if (campaigns.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.SmartGroups.AlreadyInUse"));

                return RedirectToAction("Edit", new { id = smartGroup.Id });
            }

            await _smartGroupService.DeleteSmartGroupAsync(smartGroup);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.SmartGroups.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Smart group condition 

        [HttpPost]
        public virtual async Task<IActionResult> ConditionList(SmartGroupConditionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return await AccessDeniedDataTablesJson();

            var smartGroup = await _smartGroupService.GetSmartGroupByIdAsync(searchModel.SmartGroupConditionId)
                ?? throw new ArgumentException("No product found with the specified id");

            var model = await _smartGroupModelFactory.PrepareSmartGroupConditionListModelAsync(searchModel, smartGroup);

            return Json(model);
        }

        public virtual async Task<IActionResult> CreateCondition(int smartGroupId)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var smartGroup = await _smartGroupService.GetSmartGroupByIdAsync(smartGroupId);
            if (smartGroup == null)
                return RedirectToAction("List");

            var model = await _smartGroupModelFactory.PrepareSmartGroupConditionModelAsync(smartGroup, new SmartGroupConditionModel(), null);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> CreateCondition(SmartGroupConditionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var smartGroup = await _smartGroupService.GetSmartGroupByIdAsync(model.SmartGroupId);
            if (smartGroup == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var smartGroupCondition = model.ToEntity<SmartGroupCondition>();

                await _smartGroupService.InsertSmartGroupConditionAsync(smartGroupCondition);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.SmartGroupConditions.Created"));

                if (!continueEditing)
                    return RedirectToAction("Edit", new { id = smartGroup.Id });

                return RedirectToAction("EditCondition", new { id = smartGroupCondition.Id });
            }

            model = await _smartGroupModelFactory.PrepareSmartGroupConditionModelAsync(smartGroup, new SmartGroupConditionModel(), null);

            return View(model);
        }

        public virtual async Task<IActionResult> EditCondition(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var smartGroupCondition = await _smartGroupService.GetSmartGroupConditionByIdAsync(id);
            if (smartGroupCondition == null)
                return RedirectToAction("List");

            var smartGroup = await _smartGroupService.GetSmartGroupByIdAsync(smartGroupCondition.SmartGroupId);
            var model = await _smartGroupModelFactory.PrepareSmartGroupConditionModelAsync(smartGroup, null, smartGroupCondition);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> EditCondition(SmartGroupConditionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var smartGroupCondition = await _smartGroupService.GetSmartGroupConditionByIdAsync(model.Id);
            if (smartGroupCondition == null)
                return RedirectToAction("List");

            var smartGroup = await _smartGroupService.GetSmartGroupByIdAsync(smartGroupCondition.SmartGroupId);
            if (smartGroup == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                smartGroupCondition = model.ToEntity(smartGroupCondition);
                await _smartGroupService.UpdateSmartGroupConditionAsync(smartGroupCondition);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.SmartGroupConditions.Updated"));

                if (!continueEditing)
                    return RedirectToAction("Edit", new { id = smartGroupCondition.SmartGroupId });

                return RedirectToAction("EditCondition", new { id = smartGroupCondition.Id });
            }

            model = await _smartGroupModelFactory.PrepareSmartGroupConditionModelAsync(smartGroup, null, smartGroupCondition);

            return View(model);
        }

        [EditAccess, HttpPost, ActionName("EditCondition")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnDeleteCondition")]
        public virtual async Task<IActionResult> ConditionDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageSmartGroups))
                return AccessDeniedView();

            var smartGroupCondition = await _smartGroupService.GetSmartGroupConditionByIdAsync(id);
            if (smartGroupCondition == null)
                return RedirectToAction("List");

            var smartGroupId = smartGroupCondition.SmartGroupId;

            await _smartGroupService.DeleteSmartGroupConditionAsync(smartGroupCondition);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PushNop.SmartGroupConditions.Deleted"));

            return RedirectToAction("Edit", new { id = smartGroupId });
        }

        #endregion
    }
}
