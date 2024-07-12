using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Polls;
using Nop.Services.Localization;
using Nop.Services.Polls;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Polls;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/poll/[action]")]
public partial class PollApiController : BaseAdminApiController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IPollModelFactory _pollModelFactory;
    private readonly IPollService _pollService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;

    #endregion

    #region Ctor

    public PollApiController(ILocalizationService localizationService,
        IPermissionService permissionService,
        IPollModelFactory pollModelFactory,
        IPollService pollService,
        IStoreMappingService storeMappingService,
        IStoreService storeService)
    {
        _localizationService = localizationService;
        _permissionService = permissionService;
        _pollModelFactory = pollModelFactory;
        _pollService = pollService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
    }

    #endregion

    #region Utilities

    protected virtual async Task SaveStoreMappingsAsync(Poll poll, PollModel model)
    {
        poll.LimitedToStores = model.SelectedStoreIds.Any();
        await _pollService.UpdatePollAsync(poll);

        //manage store mappings
        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(poll);
        foreach (var store in await _storeService.GetAllStoresAsync())
        {
            var existingStoreMapping = existingStoreMappings.FirstOrDefault(storeMapping => storeMapping.StoreId == store.Id);

            //new store mapping
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                if (existingStoreMapping == null)
                    await _storeMappingService.InsertStoreMappingAsync(poll, store.Id);
            }
            //or remove existing one
            else if (existingStoreMapping != null)
                await _storeMappingService.DeleteStoreMappingAsync(existingStoreMapping);
        }
    }

    #endregion

    #region Polls

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _pollModelFactory.PreparePollSearchModelAsync(new PollSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<PollSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _pollModelFactory.PreparePollListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _pollModelFactory.PreparePollModelAsync(new PollModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<PollModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var poll = model.ToEntity<Poll>();
            await _pollService.InsertPollAsync(poll);

            //save store mappings
            await SaveStoreMappingsAsync(poll, model);

            return Created(poll.Id, await _localizationService.GetResourceAsync("Admin.ContentManagement.Polls.Added"));
        }

        //prepare model
        model = await _pollModelFactory.PreparePollModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        //try to get a poll with the specified id
        var poll = await _pollService.GetPollByIdAsync(id);
        if (poll == null)
            return NotFound("No poll found with the specified id");

        //prepare model
        var model = await _pollModelFactory.PreparePollModelAsync(null, poll);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<PollModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a poll with the specified id
        var poll = await _pollService.GetPollByIdAsync(model.Id);
        if (poll == null)
            return NotFound("No poll found with the specified id");

        if (ModelState.IsValid)
        {
            poll = model.ToEntity(poll);
            await _pollService.UpdatePollAsync(poll);

            //save store mappings
            await SaveStoreMappingsAsync(poll, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.Polls.Updated"));
        }

        //prepare model
        model = await _pollModelFactory.PreparePollModelAsync(model, poll, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        //try to get a poll with the specified id
        var poll = await _pollService.GetPollByIdAsync(id);
        if (poll == null)
            return NotFound("No poll found with the specified id");

        await _pollService.DeletePollAsync(poll);

        return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.Polls.Deleted"));
    }

    #endregion

    #region Poll answer

    [HttpPost]
    public virtual async Task<IActionResult> PollAnswers([FromBody] BaseQueryModel<PollAnswerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a poll with the specified id
        var poll = await _pollService.GetPollByIdAsync(searchModel.PollId);
        if (poll == null)
            return NotFound("No poll found with the specified id");

        //prepare model
        var model = await _pollModelFactory.PreparePollAnswerListModelAsync(searchModel, poll);

        return OkWrap(model);
    }

    //ValidateAttribute is used to force model validation
    [HttpPost]
    public virtual async Task<IActionResult> PollAnswerUpdate([FromBody] BaseQueryModel<PollAnswerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        //try to get a poll answer with the specified id
        var pollAnswer = await _pollService.GetPollAnswerByIdAsync(model.Id);
        if (pollAnswer == null)
            return NotFound("No poll answer found with the specified id");

        pollAnswer = model.ToEntity(pollAnswer);

        await _pollService.UpdatePollAnswerAsync(pollAnswer);

        return Ok(defaultMessage: true);
    }

    //ValidateAttribute is used to force model validation
    [HttpPost("{pollId}")]
    public virtual async Task<IActionResult> PollAnswerAdd(int pollId, [FromBody] BaseQueryModel<PollAnswerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        //fill entity from model
        await _pollService.InsertPollAnswerAsync(model.ToEntity<PollAnswer>());

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> PollAnswerDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePolls))
            return AdminApiAccessDenied();

        //try to get a poll answer with the specified id
        var pollAnswer = await _pollService.GetPollAnswerByIdAsync(id);
        if (pollAnswer == null)
            return NotFound("No poll answer found with the specified id");

        await _pollService.DeletePollAnswerAsync(pollAnswer);

        return Ok(defaultMessage: true);
    }

    #endregion
}