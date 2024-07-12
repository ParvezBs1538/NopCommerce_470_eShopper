using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Blogs;
using Nop.Core.Events;
using Nop.Services.Blogs;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Blogs;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/blog/[action]")]
public partial class BlogApiController : BaseAdminApiController
{
    #region Fields

    private readonly IBlogModelFactory _blogModelFactory;
    private readonly IBlogService _blogService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly IUrlRecordService _urlRecordService;

    #endregion

    #region Ctor

    public BlogApiController(IBlogModelFactory blogModelFactory,
        IBlogService blogService,
        ICustomerActivityService customerActivityService,
        IEventPublisher eventPublisher,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IUrlRecordService urlRecordService)
    {
        _blogModelFactory = blogModelFactory;
        _blogService = blogService;
        _customerActivityService = customerActivityService;
        _eventPublisher = eventPublisher;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _urlRecordService = urlRecordService;
    }

    #endregion

    #region Utilities

    protected virtual async Task SaveStoreMappingsAsync(BlogPost blogPost, BlogPostModel model)
    {
        blogPost.LimitedToStores = model.SelectedStoreIds.Any();
        await _blogService.UpdateBlogPostAsync(blogPost);

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(blogPost);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                    await _storeMappingService.InsertStoreMappingAsync(blogPost, store.Id);
            }
            else
            {
                //remove store
                var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                if (storeMappingToDelete != null)
                    await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
            }
        }
    }

    #endregion

    #region Methods        

    #region Blog posts

    [HttpGet("{filterByBlogPostId?}")]
    public virtual async Task<IActionResult> BlogPosts(int? filterByBlogPostId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _blogModelFactory.PrepareBlogContentModelAsync(new BlogContentModel(), filterByBlogPostId);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<BlogPostSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _blogModelFactory.PrepareBlogPostListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> BlogPostCreate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _blogModelFactory.PrepareBlogPostModelAsync(new BlogPostModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> BlogPostCreate([FromBody] BaseQueryModel<BlogPostModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var blogPost = model.ToEntity<BlogPost>();
            blogPost.CreatedOnUtc = DateTime.UtcNow;
            await _blogService.InsertBlogPostAsync(blogPost);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewBlogPost",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewBlogPost"), blogPost.Id), blogPost);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(blogPost, model.SeName, model.Title, true);
            await _urlRecordService.SaveSlugAsync(blogPost, seName, blogPost.LanguageId);

            //Stores
            await SaveStoreMappingsAsync(blogPost, model);

            return Created(blogPost.Id, await _localizationService.GetResourceAsync("Admin.ContentManagement.Blog.BlogPosts.Added"));
        }

        //prepare model
        model = await _blogModelFactory.PrepareBlogPostModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> BlogPostEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        //try to get a blog post with the specified id
        var blogPost = await _blogService.GetBlogPostByIdAsync(id);
        if (blogPost == null)
            return NotFound("No blog post found with the specified id");

        //prepare model
        var model = await _blogModelFactory.PrepareBlogPostModelAsync(null, blogPost);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> BlogPostEdit([FromBody] BaseQueryModel<BlogPostModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a blog post with the specified id
        var blogPost = await _blogService.GetBlogPostByIdAsync(model.Id);
        if (blogPost == null)
            return NotFound("No blog post found with the specified id");

        if (ModelState.IsValid)
        {
            blogPost = model.ToEntity(blogPost);
            await _blogService.UpdateBlogPostAsync(blogPost);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditBlogPost",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditBlogPost"), blogPost.Id), blogPost);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(blogPost, model.SeName, model.Title, true);
            await _urlRecordService.SaveSlugAsync(blogPost, seName, blogPost.LanguageId);

            //Stores
            await SaveStoreMappingsAsync(blogPost, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.Blog.BlogPosts.Updated"));
        }

        //prepare model
        model = await _blogModelFactory.PrepareBlogPostModelAsync(model, blogPost, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        //try to get a blog post with the specified id
        var blogPost = await _blogService.GetBlogPostByIdAsync(id);
        if (blogPost == null)
            return NotFound("No blog post found with the specified id");

        await _blogService.DeleteBlogPostAsync(blogPost);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteBlogPost",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteBlogPost"), blogPost.Id), blogPost);

        return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.Blog.BlogPosts.Deleted"));
    }

    #endregion

    #region Comments

    [HttpGet("{filterByBlogPostId?}")]
    public virtual async Task<IActionResult> BlogComments(int? filterByBlogPostId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        //try to get a blog post with the specified id
        var blogPost = await _blogService.GetBlogPostByIdAsync(filterByBlogPostId ?? 0);
        if (blogPost == null && filterByBlogPostId.HasValue)
            return NotFound("No blog post found with the specified id");

        //prepare model
        var model = await _blogModelFactory.PrepareBlogCommentSearchModelAsync(new BlogCommentSearchModel(), blogPost);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Comments([FromBody] BaseQueryModel<BlogCommentSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _blogModelFactory.PrepareBlogCommentListModelAsync(searchModel, searchModel.BlogPostId);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CommentUpdate([FromBody] BaseQueryModel<BlogCommentModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a blog comment with the specified id
        var comment = await _blogService.GetBlogCommentByIdAsync(model.Id);
        if (comment == null)
            return NotFound("No comment found with the specified id");

        var previousIsApproved = comment.IsApproved;

        //fill entity from model
        comment = model.ToEntity(comment);

        await _blogService.UpdateBlogCommentAsync(comment);

        //raise event (only if it wasn't approved before and is approved now)
        if (!previousIsApproved && comment.IsApproved)
            await _eventPublisher.PublishAsync(new BlogCommentApprovedEvent(comment));

        //activity log
        await _customerActivityService.InsertActivityAsync("EditBlogComment",
           string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditBlogComment"), comment.Id), comment);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> CommentDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        //try to get a blog comment with the specified id
        var comment = await _blogService.GetBlogCommentByIdAsync(id);
        if (comment == null)
            return NotFound("No affiliate found with the specified id");

        await _blogService.DeleteBlogCommentAsync(comment);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteBlogPostComment",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteBlogPostComment"), comment.Id), comment);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelectedComments([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return Ok(defaultMessage: true);

        var comments = await _blogService.GetBlogCommentsByIdsAsync(selectedIds.ToArray());

        await _blogService.DeleteBlogCommentsAsync(comments);
        //activity log
        foreach (var blogComment in comments)
        {
            await _customerActivityService.InsertActivityAsync("DeleteBlogPostComment",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteBlogPostComment"), blogComment.Id), blogComment);
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ApproveSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return Ok(defaultMessage: true);

        //filter not approved comments
        var blogComments = (await _blogService.GetBlogCommentsByIdsAsync(selectedIds.ToArray())).Where(comment => !comment.IsApproved);

        foreach (var blogComment in blogComments)
        {
            blogComment.IsApproved = true;

            await _blogService.UpdateBlogCommentAsync(blogComment);

            //raise event 
            await _eventPublisher.PublishAsync(new BlogCommentApprovedEvent(blogComment));

            //activity log
            await _customerActivityService.InsertActivityAsync("EditBlogComment",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditBlogComment"), blogComment.Id), blogComment);
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DisapproveSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return Ok(defaultMessage: true);

        //filter approved comments
        var blogComments = (await _blogService.GetBlogCommentsByIdsAsync(selectedIds.ToArray())).Where(comment => comment.IsApproved);

        foreach (var blogComment in blogComments)
        {
            blogComment.IsApproved = false;

            await _blogService.UpdateBlogCommentAsync(blogComment);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditBlogComment",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditBlogComment"), blogComment.Id), blogComment);
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #endregion
}