using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Factories;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Domain;
using NopStation.Plugin.Misc.SqlManager.Services;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Controllers
{
    public class SqlParameterController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISqlParameterModelFactory _sqlParameterModelFactory;
        private readonly ISqlParameterService _sqlParameterService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public SqlParameterController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISqlParameterModelFactory sqlParameterModelFactory,
            ISqlParameterService sqlParameterService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _sqlParameterModelFactory = sqlParameterModelFactory;
            _sqlParameterService = sqlParameterService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods        

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return AccessDeniedView();

            var searchModel = await _sqlParameterModelFactory.PrepareSqlParameterSearchModelAsync(new SqlParameterSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> List(SqlParameterSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return await AccessDeniedDataTablesJson();

            var model = await _sqlParameterModelFactory.PrepareSqlParameterListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return AccessDeniedView();

            var model = await _sqlParameterModelFactory.PrepareSqlParameterModelAsync(new SqlParameterModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(SqlParameterModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return AccessDeniedView();

            var existingParameter = await _sqlParameterService.GetSqlParameterBySystemNameAsync(model.SystemName);
            if (existingParameter != null)
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlParameters.AlreadyExists"));

            if (ModelState.IsValid)
            {
                var sqlParameter = model.ToEntity<SqlParameter>();
                await _sqlParameterService.InsertSqlParameterAsync(sqlParameter);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlParameters.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = sqlParameter.Id });
            }

            model = await _sqlParameterModelFactory.PrepareSqlParameterModelAsync(new SqlParameterModel(), null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return AccessDeniedView();

            var sqlParameter = await _sqlParameterService.GetSqlParameterByIdAsync(id);
            if (sqlParameter == null)
                return RedirectToAction("List");

            var model = await _sqlParameterModelFactory.PrepareSqlParameterModelAsync(null, sqlParameter);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(SqlParameterModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return AccessDeniedView();

            var sqlParameter = await _sqlParameterService.GetSqlParameterByIdAsync(model.Id);
            if (sqlParameter == null)
                return RedirectToAction("List");

            var existingParameter = _sqlParameterService.IsSystemNameExsistForAnotherSqlParameter(model.SystemName, sqlParameter.Id);
            if (existingParameter)
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlParameters.AlreadyExists"));

            if (ModelState.IsValid)
            {
                sqlParameter = model.ToEntity(sqlParameter);
                await _sqlParameterService.UpdateSqlParameterAsync(sqlParameter);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlParameters.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = sqlParameter.Id });
            }

            model = await _sqlParameterModelFactory.PrepareSqlParameterModelAsync(model, sqlParameter);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return AccessDeniedView();

            var sqlParameter = await _sqlParameterService.GetSqlParameterByIdAsync(id);
            if (sqlParameter == null)
                return RedirectToAction("List");

            await _sqlParameterService.DeleteSqlParameterAsync(sqlParameter);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlParameters.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> ValueList(SqlParameterValueSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return AccessDeniedView();

            var sqlParameter = await _sqlParameterService.GetSqlParameterByIdAsync(searchModel.SqlParameterId);
            if (sqlParameter == null)
                return RedirectToAction("List");

            var model = await _sqlParameterModelFactory.PrepareSqlParameterValueListModelAsync(searchModel, sqlParameter);
            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> ValueCreate(SqlParameterValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return Json(new { Result = false, Message = "Access denied" });

            var sqlParameter = await _sqlParameterService.GetSqlParameterByIdAsync(model.SqlParameterId);
            if (sqlParameter == null)
                return Json(new { Result = false, Message = "No sql parameter found with the specified id" });

            if (sqlParameter.DataType == DataType.Date || sqlParameter.DataType == DataType.InputText)
                return Json(new { Result = false, Message = "No need for this type of parameter." });

            if (string.IsNullOrWhiteSpace(model.Value))
                return ErrorJson(await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlParameterValues.Fields.Value.Required"));

            var sqlParameterValue = model.ToEntity<SqlParameterValue>();
            await _sqlParameterService.InsertSqlParameterValueAsync(sqlParameterValue);

            return Json(new { Result = true });
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> ValueEdit(SqlParameterValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return await AccessDeniedDataTablesJson();

            var sqlParameterValue = await _sqlParameterService.GetSqlParameterValueByIdAsync(model.Id);
            if (sqlParameterValue == null)
                return ErrorJson("No sql parameter value found with the specified id");

            if (string.IsNullOrWhiteSpace(model.Value))
                return ErrorJson(await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlParameterValues.Fields.Value.Required"));

            sqlParameterValue.Value = model.Value;
            await _sqlParameterService.UpdateSqlParameterValueAsync(sqlParameterValue);

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> ValueDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
                return await AccessDeniedDataTablesJson();

            var sqlParameterValue = await _sqlParameterService.GetSqlParameterValueByIdAsync(id);
            if (sqlParameterValue == null)
                return ErrorJson("No sql parameter value found with the specified id");

            await _sqlParameterService.DeleteSqlParameterValueAsync(sqlParameterValue);

            return new NullJsonResult();
        }

        #endregion
    }
}