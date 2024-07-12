using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Services;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Controllers
{
    public class InstantQueryController : NopStationAdminController
    {
        #region Fields

        private readonly ISqlReportService _sqlReportService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region ctor

        public InstantQueryController(ISqlReportService sqlReportService,
            IPermissionService permissionService)
        {
            _sqlReportService = sqlReportService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods 

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
                return AccessDeniedView();

            return RedirectToAction("RunQuery");
        }

        public async Task<IActionResult> RunQuery()
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ViewReports))
                return AccessDeniedView();

            var model = new SqlQueryModel();
            return View(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> RunQuery(SqlQueryModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ViewReports))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            try
            {
                var result = await _sqlReportService.RunQueryAsync(model);
                return Json(new { Result = true, outputlist = result.Results, message = result.Message, rowreturned = result.ReturnedRow });
            }
            catch (Exception ex)
            {
                return Json(new { Result = true, message = ex.Message });
            }
        }

        #endregion
    }
}
