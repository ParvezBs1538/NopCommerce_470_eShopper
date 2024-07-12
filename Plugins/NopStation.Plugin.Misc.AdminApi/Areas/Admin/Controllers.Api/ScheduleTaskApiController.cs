using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Tasks;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/scheduletask/[action]")]
public partial class ScheduleTaskApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IScheduleTaskModelFactory _scheduleTaskModelFactory;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly IScheduleTaskRunner _taskRunner;

    #endregion

    #region Ctor

    public ScheduleTaskApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IScheduleTaskModelFactory scheduleTaskModelFactory,
        IScheduleTaskService scheduleTaskService,
         IScheduleTaskRunner taskRunner)
    {
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _scheduleTaskModelFactory = scheduleTaskModelFactory;
        _scheduleTaskService = scheduleTaskService;
        _taskRunner = taskRunner;
    }

    #endregion

    #region Methods

    [HttpGet]
    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _scheduleTaskModelFactory.PrepareScheduleTaskSearchModelAsync(new ScheduleTaskSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<ScheduleTaskSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _scheduleTaskModelFactory.PrepareScheduleTaskListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TaskUpdate([FromBody] BaseQueryModel<ScheduleTaskModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a schedule task with the specified id
        var scheduleTask = await _scheduleTaskService.GetTaskByIdAsync(model.Id);
        if (scheduleTask == null)
            return NotFound("No schedule task found with the specified id");

        //To prevent inject the XSS payload in Schedule tasks ('Name' field), we must disable editing this field, 
        //but since it is required, we need to get its value before updating the entity.
        if (!string.IsNullOrEmpty(scheduleTask.Name))
        {
            model.Name = scheduleTask.Name;
            ModelState.Remove(nameof(model.Name));
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState.SerializeErrors());

        if (!scheduleTask.Enabled && model.Enabled)
            scheduleTask.LastEnabledUtc = DateTime.UtcNow;

        scheduleTask = model.ToEntity(scheduleTask);

        await _scheduleTaskService.UpdateTaskAsync(scheduleTask);

        //activity log
        await _customerActivityService.InsertActivityAsync("EditTask",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditTask"), scheduleTask.Id), scheduleTask);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> RunNow(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
            return AdminApiAccessDenied();

        try
        {
            //try to get a schedule task with the specified id
            var scheduleTask = await _scheduleTaskService.GetTaskByIdAsync(id)
                               ?? throw new ArgumentException("Schedule task cannot be loaded", nameof(id));

            await _taskRunner.ExecuteAsync(scheduleTask, true, true, false);

            return Ok(await _localizationService.GetResourceAsync("Admin.System.ScheduleTasks.RunNow.Done"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    #endregion
}