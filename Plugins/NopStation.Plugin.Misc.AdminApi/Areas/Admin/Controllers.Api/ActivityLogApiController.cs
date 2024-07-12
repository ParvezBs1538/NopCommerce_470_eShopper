using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Logging;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/activitylog/[action]")]
public partial class ActivityLogApiController : BaseAdminApiController
{
    #region Fields

    private readonly IActivityLogModelFactory _activityLogModelFactory;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public ActivityLogApiController(IActivityLogModelFactory activityLogModelFactory,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        IPermissionService permissionService)
    {
        _activityLogModelFactory = activityLogModelFactory;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> ActivityTypes()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return AdminApiAccessDenied();

        var model = await _activityLogModelFactory.PrepareActivityLogTypeSearchModelAsync(new ActivityLogTypeSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SaveTypes([FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return AdminApiAccessDenied();

        await _customerActivityService.InsertActivityAsync("EditActivityLogTypes", await _localizationService.GetResourceAsync("ActivityLog.EditActivityLogTypes"));

        var form = queryModel.FormValues.ToNameValueCollection();
        var selectedActivityTypesIds = form.GetFormValues<string>("checkbox_activity_types")
            .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(idString => int.TryParse(idString, out var id) ? id : 0)
            .Distinct().ToList();
        ;

        var activityTypes = await _customerActivityService.GetAllActivityTypesAsync();
        foreach (var activityType in activityTypes)
        {
            activityType.Enabled = selectedActivityTypesIds.Contains(activityType.Id);
            await _customerActivityService.UpdateActivityTypeAsync(activityType);
        }

        return Ok(await _localizationService.GetResourceAsync("Admin.Customers.ActivityLogType.Updated"));
    }

    public virtual async Task<IActionResult> ActivityLogs()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return AdminApiAccessDenied();

        var model = await _activityLogModelFactory.PrepareActivityLogSearchModelAsync(new ActivityLogSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ListLogs([FromBody] BaseQueryModel<ActivityLogSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        var model = await _activityLogModelFactory.PrepareActivityLogListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ActivityLogDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return AdminApiAccessDenied();

        var logItem = await _customerActivityService.GetActivityByIdAsync(id);
        if (logItem == null)
            return NotFound("No activity log found with the specified id");

        await _customerActivityService.DeleteActivityAsync(logItem);

        await _customerActivityService.InsertActivityAsync("DeleteActivityLog",
            await _localizationService.GetResourceAsync("ActivityLog.DeleteActivityLog"), logItem);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ClearAll()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return AdminApiAccessDenied();

        await _customerActivityService.ClearAllActivitiesAsync();

        await _customerActivityService.InsertActivityAsync("DeleteActivityLog", await _localizationService.GetResourceAsync("ActivityLog.DeleteActivityLog"));

        return Ok(defaultMessage: true);
    }

    #endregion
}