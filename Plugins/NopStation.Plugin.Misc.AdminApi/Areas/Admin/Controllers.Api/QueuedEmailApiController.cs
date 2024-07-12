using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Messages;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/queuedemail/[action]")]
public partial class QueuedEmailApiController : BaseAdminApiController
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IQueuedEmailModelFactory _queuedEmailModelFactory;
    private readonly IQueuedEmailService _queuedEmailService;

    #endregion

    #region Ctor

    public QueuedEmailApiController(IDateTimeHelper dateTimeHelper,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IQueuedEmailModelFactory queuedEmailModelFactory,
        IQueuedEmailService queuedEmailService)
    {
        _dateTimeHelper = dateTimeHelper;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _queuedEmailModelFactory = queuedEmailModelFactory;
        _queuedEmailService = queuedEmailService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageQueue))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _queuedEmailModelFactory.PrepareQueuedEmailSearchModelAsync(new QueuedEmailSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<QueuedEmailSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageQueue))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _queuedEmailModelFactory.PrepareQueuedEmailListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GoToEmailByNumber([FromBody] BaseQueryModel<QueuedEmailSearchModel> queryModel)
    {
        var model = queryModel.Data;
        //try to get a queued email with the specified id
        var queuedEmail = await _queuedEmailService.GetQueuedEmailByIdAsync(model.GoDirectlyToNumber);
        if (queuedEmail == null)
            return NotFound("No queued email found with the specified id");

        return await Edit(queuedEmail.Id);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageQueue))
            return AdminApiAccessDenied();

        //try to get a queued email with the specified id
        var email = await _queuedEmailService.GetQueuedEmailByIdAsync(id);
        if (email == null)
            return NotFound("No queued email found with the specified id");

        //prepare model
        var model = await _queuedEmailModelFactory.PrepareQueuedEmailModelAsync(null, email);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<QueuedEmailModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageQueue))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a queued email with the specified id
        var email = await _queuedEmailService.GetQueuedEmailByIdAsync(model.Id);
        if (email == null)
            return NotFound("No queued email found with the specified id");

        if (ModelState.IsValid)
        {
            email = model.ToEntity(email);
            email.DontSendBeforeDateUtc = model.SendImmediately || !model.DontSendBeforeDate.HasValue ?
                null : _dateTimeHelper.ConvertToUtcTime(model.DontSendBeforeDate.Value);
            await _queuedEmailService.UpdateQueuedEmailAsync(email);

            return Ok(await _localizationService.GetResourceAsync("Admin.System.QueuedEmails.Updated"));
        }

        //prepare model
        model = await _queuedEmailModelFactory.PrepareQueuedEmailModelAsync(model, email, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Requeue([FromBody] BaseQueryModel<QueuedEmailModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageQueue))
            return AdminApiAccessDenied();

        var queuedEmailModel = queryModel.Data;
        //try to get a queued email with the specified id
        var queuedEmail = await _queuedEmailService.GetQueuedEmailByIdAsync(queuedEmailModel.Id);
        if (queuedEmail == null)
            return NotFound("No queued email found with the specified id");

        var requeuedEmail = new QueuedEmail
        {
            PriorityId = queuedEmail.PriorityId,
            From = queuedEmail.From,
            FromName = queuedEmail.FromName,
            To = queuedEmail.To,
            ToName = queuedEmail.ToName,
            ReplyTo = queuedEmail.ReplyTo,
            ReplyToName = queuedEmail.ReplyToName,
            CC = queuedEmail.CC,
            Bcc = queuedEmail.Bcc,
            Subject = queuedEmail.Subject,
            Body = queuedEmail.Body,
            AttachmentFilePath = queuedEmail.AttachmentFilePath,
            AttachmentFileName = queuedEmail.AttachmentFileName,
            AttachedDownloadId = queuedEmail.AttachedDownloadId,
            CreatedOnUtc = DateTime.UtcNow,
            EmailAccountId = queuedEmail.EmailAccountId,
            DontSendBeforeDateUtc = queuedEmailModel.SendImmediately || !queuedEmailModel.DontSendBeforeDate.HasValue ?
                null : _dateTimeHelper.ConvertToUtcTime(queuedEmailModel.DontSendBeforeDate.Value)
        };
        await _queuedEmailService.InsertQueuedEmailAsync(requeuedEmail);

        return Created(queuedEmail.Id, await _localizationService.GetResourceAsync("Admin.System.QueuedEmails.Requeued"));
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageQueue))
            return AdminApiAccessDenied();

        //try to get a queued email with the specified id
        var email = await _queuedEmailService.GetQueuedEmailByIdAsync(id);
        if (email == null)
            return NotFound("No queued email found with the specified id");

        await _queuedEmailService.DeleteQueuedEmailAsync(email);

        return Ok(await _localizationService.GetResourceAsync("Admin.System.QueuedEmails.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageQueue))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return NotFound("Not found with specified id");

        await _queuedEmailService.DeleteQueuedEmailsAsync(await _queuedEmailService.GetQueuedEmailsByIdsAsync(selectedIds.ToArray()));

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteAll()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageQueue))
            return AdminApiAccessDenied();

        await _queuedEmailService.DeleteAllEmailsAsync();

        return Ok(await _localizationService.GetResourceAsync("Admin.System.QueuedEmails.DeletedAll"));
    }

    #endregion
}