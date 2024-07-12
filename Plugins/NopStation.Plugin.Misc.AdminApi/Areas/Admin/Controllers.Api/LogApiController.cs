using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Logging;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/log/[action]")]
public partial class LogApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private readonly ILogModelFactory _logModelFactory;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public LogApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILogger logger,
        ILogModelFactory logModelFactory,
        IPermissionService permissionService)
    {
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _logger = logger;
        _logModelFactory = logModelFactory;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _logModelFactory.PrepareLogSearchModelAsync(new LogSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> LogList([FromBody] BaseQueryModel<LogSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _logModelFactory.PrepareLogListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ClearAll()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
            return AdminApiAccessDenied();

        await _logger.ClearLogAsync();

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteSystemLog", await _localizationService.GetResourceAsync("ActivityLog.DeleteSystemLog"));

        return Ok(await _localizationService.GetResourceAsync("Admin.System.Log.Cleared"));
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> View(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
            return AdminApiAccessDenied();

        //try to get a log with the specified id
        var log = await _logger.GetLogByIdAsync(id);
        if (log == null)
            return NotFound("No log found with the specified id");

        //prepare model
        var model = await _logModelFactory.PrepareLogModelAsync(null, log);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
            return AdminApiAccessDenied();

        //try to get a log with the specified id
        var log = await _logger.GetLogByIdAsync(id);
        if (log == null)
            return NotFound("No log found with the specified id");

        await _logger.DeleteLogAsync(log);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteSystemLog", await _localizationService.GetResourceAsync("ActivityLog.DeleteSystemLog"), log);

        return Ok(await _localizationService.GetResourceAsync("Admin.System.Log.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return NotFound();

        await _logger.DeleteLogsAsync((await _logger.GetLogByIdsAsync(selectedIds.ToArray())).ToList());

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteSystemLog", await _localizationService.GetResourceAsync("ActivityLog.DeleteSystemLog"));

        return Ok(defaultMessage: true);
    }

    #endregion
}