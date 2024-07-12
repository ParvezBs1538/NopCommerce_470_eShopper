using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Forums;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Forums;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/forum/[action]")]
public partial class ForumApiController : BaseAdminApiController
{
    #region Fields

    private readonly IForumModelFactory _forumModelFactory;
    private readonly IForumService _forumService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public ForumApiController(IForumModelFactory forumModelFactory,
        IForumService forumService,
        ILocalizationService localizationService,
        IPermissionService permissionService)
    {
        _forumModelFactory = forumModelFactory;
        _forumService = forumService;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    #region List

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _forumModelFactory.PrepareForumGroupSearchModelAsync(new ForumGroupSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ForumGroupList([FromBody] BaseQueryModel<ForumGroupSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _forumModelFactory.PrepareForumGroupListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ForumList([FromBody] BaseQueryModel<ForumSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a forum group with the specified id
        var forumGroup = await _forumService.GetForumGroupByIdAsync(searchModel.ForumGroupId);
        if (forumGroup == null)
            return NotFound("No forum group found with the specified id");

        //prepare model
        var model = await _forumModelFactory.PrepareForumListModelAsync(searchModel, forumGroup);

        return OkWrap(model);
    }

    #endregion

    #region Create

    public virtual async Task<IActionResult> CreateForumGroup()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _forumModelFactory.PrepareForumGroupModelAsync(new ForumGroupModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateForumGroup([FromBody] BaseQueryModel<ForumGroupModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var forumGroup = model.ToEntity<ForumGroup>();
            forumGroup.CreatedOnUtc = DateTime.UtcNow;
            forumGroup.UpdatedOnUtc = DateTime.UtcNow;
            await _forumService.InsertForumGroupAsync(forumGroup);

            return Created(forumGroup.Id, await _localizationService.GetResourceAsync("Admin.ContentManagement.Forums.ForumGroup.Added"));
        }

        //prepare model
        model = await _forumModelFactory.PrepareForumGroupModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> CreateForum()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _forumModelFactory.PrepareForumModelAsync(new ForumModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateForum([FromBody] BaseQueryModel<ForumModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var forum = model.ToEntity<Forum>();
            forum.CreatedOnUtc = DateTime.UtcNow;
            forum.UpdatedOnUtc = DateTime.UtcNow;
            await _forumService.InsertForumAsync(forum);

            return Created(forum.Id, await _localizationService.GetResourceAsync("Admin.ContentManagement.Forums.Forum.Added"));
        }

        //prepare model
        model = await _forumModelFactory.PrepareForumModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    #endregion

    #region Edit

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditForumGroup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        //try to get a forum group with the specified id
        var forumGroup = await _forumService.GetForumGroupByIdAsync(id);
        if (forumGroup == null)
            return NotFound("No forum group found with the specified id");

        //prepare model
        var model = await _forumModelFactory.PrepareForumGroupModelAsync(null, forumGroup);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditForumGroup([FromBody] BaseQueryModel<ForumGroupModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a forum group with the specified id
        var forumGroup = await _forumService.GetForumGroupByIdAsync(model.Id);
        if (forumGroup == null)
            return NotFound("No forum group found with the specified id");

        if (ModelState.IsValid)
        {
            forumGroup = model.ToEntity(forumGroup);
            forumGroup.UpdatedOnUtc = DateTime.UtcNow;
            await _forumService.UpdateForumGroupAsync(forumGroup);

            return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.Forums.ForumGroup.Updated"));
        }

        //prepare model
        model = await _forumModelFactory.PrepareForumGroupModelAsync(model, forumGroup, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditForum(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        //try to get a forum with the specified id
        var forum = await _forumService.GetForumByIdAsync(id);
        if (forum == null)
            return NotFound("No forum found with the specified id");

        //prepare model
        var model = await _forumModelFactory.PrepareForumModelAsync(null, forum);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditForum([FromBody] BaseQueryModel<ForumModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a forum with the specified id
        var forum = await _forumService.GetForumByIdAsync(model.Id);
        if (forum == null)
            return NotFound("No forum found with the specified id");

        if (ModelState.IsValid)
        {
            forum = model.ToEntity(forum);
            forum.UpdatedOnUtc = DateTime.UtcNow;
            await _forumService.UpdateForumAsync(forum);

            return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.Forums.Forum.Updated"));
        }

        //prepare model
        model = await _forumModelFactory.PrepareForumModelAsync(model, forum, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    #endregion

    #region Delete

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteForumGroup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        //try to get a forum group with the specified id
        var forumGroup = await _forumService.GetForumGroupByIdAsync(id);
        if (forumGroup == null)
            return NotFound("No forum group found with the specified id");

        await _forumService.DeleteForumGroupAsync(forumGroup);

        return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.Forums.ForumGroup.Deleted"));
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteForum(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageForums))
            return AdminApiAccessDenied();

        //try to get a forum with the specified id
        var forum = await _forumService.GetForumByIdAsync(id);
        if (forum == null)
            return NotFound("No forum found with the specified id");

        await _forumService.DeleteForumAsync(forum);

        return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.Forums.Forum.Deleted"));
    }

    #endregion

    #endregion
}