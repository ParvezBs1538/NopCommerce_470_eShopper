using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Factories;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Domain;
using NopStation.Plugin.Misc.SqlManager.Services;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Controllers
{
    public class SqlReportController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISqlReportModelFactory _sqlReportModelFactory;
        private readonly ISqlReportService _sqlReportService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IAclService _aclService;
        private readonly ISqlParameterService _sqlParameterService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public SqlReportController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISqlReportModelFactory sqlReportModelFactory,
            ISqlReportService sqlReportService,
            IPermissionService permissionService,
            IWorkContext workContext,
            ICustomerService customerService,
            IAclService aclService,
            ISqlParameterService sqlParameterService,
            ILogger logger)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _sqlReportModelFactory = sqlReportModelFactory;
            _sqlReportService = sqlReportService;
            _permissionService = permissionService;
            _workContext = workContext;
            _customerService = customerService;
            _aclService = aclService;
            _sqlParameterService = sqlParameterService;
            _logger = logger;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveSqlReportAclAsync(SqlReport sqlReport, SqlReportModel model)
        {
            sqlReport.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _sqlReportService.UpdateSqlReportAsync(sqlReport);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(sqlReport);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        await _aclService.InsertAclRecordAsync(sqlReport, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        protected async Task<IList<SqlReportFilterOptionModel>> ParseFormAsync(IFormCollection form)
        {
            var list = new List<SqlReportFilterOptionModel>();
            foreach (var key in form.Keys)
            {
                if (!key.StartsWith("filterItem_"))
                    continue;

                var tokens = key.Split('_');
                if (tokens.Length != 3)
                    continue;

                var val = form[key];
                var parameter = await _sqlParameterService.GetSqlParameterBySystemNameAsync(tokens[1]);
                if (parameter == null)
                    continue;

                var parameterValue = "";

                switch (parameter.DataType)
                {
                    case DataType.Text:
                    case DataType.InputText:
                        parameterValue = val.FirstOrDefault();
                        break;
                    case DataType.Number:
                        if (decimal.TryParse(val.FirstOrDefault(), out var numVal))
                            parameterValue = Convert.ToString(numVal);
                        else
                            continue;
                        break;
                    case DataType.TextList:
                        var textListVal = val.Any() ? val.ToList() : new List<string>();
                        parameterValue = string.Join(",", textListVal.Select(x => "'" + x + "'"));
                        break;
                    case DataType.NumberList:
                        var numListVal = val.Any() ? val.Select(int.Parse).ToList() : new List<int>();
                        parameterValue = string.Join(",", numListVal);
                        break;
                    case DataType.Date:
                        parameterValue = "'" + val.FirstOrDefault() + "'";
                        break;

                    default:
                        continue;
                }

                list.Add(new SqlReportFilterOptionModel()
                {
                    SystemName = tokens[1],
                    Order = int.Parse(tokens[2]),
                    FormattedValue = parameterValue
                });
            }

            list = list.OrderBy(x => x.Order).ToList();

            return list;
        }

        private string PrepareSqlQuery(string sql, IList<SqlReportFilterOptionModel> appliedFilters)
        {
            foreach (var filter in appliedFilters)
            {
                var regex = new Regex(Regex.Escape("@P(" + filter.SystemName + ")"));
                sql = regex.Replace(sql, filter.FormattedValue, 1);
            }

            return sql;
        }

        #endregion

        #region Methods        

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return AccessDeniedView();

            var searchModel = await _sqlReportModelFactory.PrepareSqlReportSearchModelAsync(new SqlReportSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> List(SqlReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return await AccessDeniedDataTablesJson();

            var model = await _sqlReportModelFactory.PrepareSqlReportListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return AccessDeniedView();

            var model = await _sqlReportModelFactory.PrepareSqlReportModelAsync(new SqlReportModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(SqlReportModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var sqlReport = model.ToEntity<SqlReport>();
                sqlReport.CreatedOnUtc = DateTime.UtcNow;
                sqlReport.UpdatedOnUtc = DateTime.UtcNow;

                await _sqlReportService.InsertSqlReportAsync(sqlReport);
                await SaveSqlReportAclAsync(sqlReport, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlReports.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = sqlReport.Id });
            }

            model = await _sqlReportModelFactory.PrepareSqlReportModelAsync(model, null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return AccessDeniedView();

            var sqlReport = await _sqlReportService.GetSqlReportByIdAsync(id);
            if (sqlReport == null)
                return RedirectToAction("List");

            var model = await _sqlReportModelFactory.PrepareSqlReportModelAsync(null, sqlReport);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(SqlReportModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return AccessDeniedView();

            var sqlReport = await _sqlReportService.GetSqlReportByIdAsync(model.Id);
            if (sqlReport == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                sqlReport = model.ToEntity(sqlReport);
                sqlReport.UpdatedOnUtc = DateTime.UtcNow;

                await _sqlReportService.UpdateSqlReportAsync(sqlReport);
                await SaveSqlReportAclAsync(sqlReport, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlReports.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = sqlReport.Id });
            }

            model = await _sqlReportModelFactory.PrepareSqlReportModelAsync(model, sqlReport);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return AccessDeniedView();

            var sqlReport = await _sqlReportService.GetSqlReportByIdAsync(id);
            if (sqlReport == null)
                return RedirectToAction("List");

            await _sqlReportService.DeleteSqlReportAsync(sqlReport);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.SqlReports.Deleted"));

            return RedirectToAction("List");
        }

        public async Task<IActionResult> ViewList()
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ViewReports))
                return AccessDeniedView();

            var searchModel = await _sqlReportModelFactory.PrepareSqlReportSearchModelAsync(new SqlReportSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> ViewList(SqlReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return await AccessDeniedDataTablesJson();

            searchModel.SelectedCustomerRoleIds = (await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync())).ToList();

            var model = await _sqlReportModelFactory.PrepareSqlReportListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> ViewReport(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ViewReports))
                return AccessDeniedView();

            var sqlReport = await _sqlReportService.GetSqlReportByIdAsync(id);
            if (sqlReport == null)
                return RedirectToAction("ViewList");

            if (!await _sqlReportService.AuthorizeAsync(sqlReport))
                return RedirectToAction("ViewList");

            var model = await _sqlReportModelFactory.PrepareSqlReportViewModelAsync(sqlReport);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ViewReport(int id, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ViewReports))
                return Json(new { Result = false, Message = "Access denied" });

            var sqlReport = await _sqlReportService.GetSqlReportByIdAsync(id);
            if (sqlReport == null)
                return Json(new { Result = false, Message = "No sql report found with the specified id" });

            if (!await _sqlReportService.AuthorizeAsync(sqlReport))
                return Json(new { Result = false, Message = "Access denied" });

            var model = await ParseFormAsync(form);

            try
            {
                var sql = PrepareSqlQuery(sqlReport.Query, model);
                var result = await _sqlReportService.RunQueryAsync(sql);
                if (result.Count <= 0)
                {
                    return Json(new { Result = false, Message = "No data found" });
                }
                return Json(new { Result = true, result });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return Json(new { message = ex.Message });
            }
        }

        [EditAccess, HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ExportReport(IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ViewReports))
                return AccessDeniedView();

            var id = Convert.ToInt32(form["Id"]);
            var sqlReport = await _sqlReportService.GetSqlReportByIdAsync(id);
            if (sqlReport == null)
            {
                return BadRequest("Report does not exist.");
            }

            var model = await ParseFormAsync(form);
            var sql = PrepareSqlQuery(sqlReport.Query, model);
            var result = await _sqlReportService.ExportToExcelAsync(sql);

            return File(result, "text/xls", $"{sqlReport.Name ?? "Report"}.xlsx");
        }

        #endregion
    }
}
