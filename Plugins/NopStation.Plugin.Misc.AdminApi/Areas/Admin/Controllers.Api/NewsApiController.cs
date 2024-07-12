using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.News;
using Nop.Core.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.News;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.News;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/news/[action]")]
public partial class NewsApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILocalizationService _localizationService;
    private readonly INewsModelFactory _newsModelFactory;
    private readonly INewsService _newsService;
    private readonly IPermissionService _permissionService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly IUrlRecordService _urlRecordService;

    #endregion

    #region Ctor

    public NewsApiController(ICustomerActivityService customerActivityService,
        IEventPublisher eventPublisher,
        ILocalizationService localizationService,
        INewsModelFactory newsModelFactory,
        INewsService newsService,
        IPermissionService permissionService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IUrlRecordService urlRecordService)
    {
        _customerActivityService = customerActivityService;
        _eventPublisher = eventPublisher;
        _localizationService = localizationService;
        _newsModelFactory = newsModelFactory;
        _newsService = newsService;
        _permissionService = permissionService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _urlRecordService = urlRecordService;
    }

    #endregion

    #region Utilities

    protected virtual async Task SaveStoreMappingsAsync(NewsItem newsItem, NewsItemModel model)
    {
        newsItem.LimitedToStores = model.SelectedStoreIds.Any();
        await _newsService.UpdateNewsAsync(newsItem);

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(newsItem);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                    await _storeMappingService.InsertStoreMappingAsync(newsItem, store.Id);
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

    #region News items

    [HttpGet("{filterByNewsItemId?}")]
    public virtual async Task<IActionResult> NewsItems(int? filterByNewsItemId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _newsModelFactory.PrepareNewsContentModelAsync(new NewsContentModel(), filterByNewsItemId);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<NewsItemSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _newsModelFactory.PrepareNewsItemListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> NewsItemCreate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _newsModelFactory.PrepareNewsItemModelAsync(new NewsItemModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> NewsItemCreate([FromBody] BaseQueryModel<NewsItemModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var newsItem = model.ToEntity<NewsItem>();
            newsItem.CreatedOnUtc = DateTime.UtcNow;
            await _newsService.InsertNewsAsync(newsItem);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewNews",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewNews"), newsItem.Id), newsItem);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(newsItem, model.SeName, model.Title, true);
            await _urlRecordService.SaveSlugAsync(newsItem, seName, newsItem.LanguageId);

            //Stores
            await SaveStoreMappingsAsync(newsItem, model);

            return Created(newsItem.Id, await _localizationService.GetResourceAsync("Admin.ContentManagement.News.NewsItems.Added"));
        }

        //prepare model
        model = await _newsModelFactory.PrepareNewsItemModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> NewsItemEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        //try to get a news item with the specified id
        var newsItem = await _newsService.GetNewsByIdAsync(id);
        if (newsItem == null)
            return NotFound("No news item found with the specified id");

        //prepare model
        var model = await _newsModelFactory.PrepareNewsItemModelAsync(null, newsItem);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> NewsItemEdit([FromBody] BaseQueryModel<NewsItemModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a news item with the specified id
        var newsItem = await _newsService.GetNewsByIdAsync(model.Id);
        if (newsItem == null)
            return NotFound("No news item found with the specified id");

        if (ModelState.IsValid)
        {
            newsItem = model.ToEntity(newsItem);
            await _newsService.UpdateNewsAsync(newsItem);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditNews",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditNews"), newsItem.Id), newsItem);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(newsItem, model.SeName, model.Title, true);
            await _urlRecordService.SaveSlugAsync(newsItem, seName, newsItem.LanguageId);

            //stores
            await SaveStoreMappingsAsync(newsItem, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.News.NewsItems.Updated"));
        }

        //prepare model
        model = await _newsModelFactory.PrepareNewsItemModelAsync(model, newsItem, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        //try to get a news item with the specified id
        var newsItem = await _newsService.GetNewsByIdAsync(id);
        if (newsItem == null)
            return NotFound("No news item found with the specified id");

        await _newsService.DeleteNewsAsync(newsItem);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteNews",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteNews"), newsItem.Id), newsItem);

        return Ok(await _localizationService.GetResourceAsync("Admin.ContentManagement.News.NewsItems.Deleted"));
    }

    #endregion

    #region Comments

    [HttpGet("{filterByNewsItemId?}")]
    public virtual async Task<IActionResult> NewsComments(int? filterByNewsItemId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        //try to get a news item with the specified id
        var newsItem = await _newsService.GetNewsByIdAsync(filterByNewsItemId ?? 0);
        if (newsItem == null && filterByNewsItemId.HasValue)
            return NotFound("No news item found with the specified id");

        //prepare model
        var model = await _newsModelFactory.PrepareNewsCommentSearchModelAsync(new NewsCommentSearchModel(), newsItem);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Comments([FromBody] BaseQueryModel<NewsCommentSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _newsModelFactory.PrepareNewsCommentListModelAsync(searchModel, searchModel.NewsItemId);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CommentUpdate([FromBody] BaseQueryModel<NewsCommentModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a news comment with the specified id
        var comment = await _newsService.GetNewsCommentByIdAsync(model.Id);
        if (comment == null)
            return NotFound("No comment found with the specified id");

        var previousIsApproved = comment.IsApproved;

        //fill entity from model
        comment = model.ToEntity(comment);

        await _newsService.UpdateNewsCommentAsync(comment);

        //activity log
        await _customerActivityService.InsertActivityAsync("EditNewsComment",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditNewsComment"), comment.Id), comment);

        //raise event (only if it wasn't approved before and is approved now)
        if (!previousIsApproved && comment.IsApproved)
            await _eventPublisher.PublishAsync(new NewsCommentApprovedEvent(comment));

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> CommentDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        //try to get a news comment with the specified id
        var comment = await _newsService.GetNewsCommentByIdAsync(id);
        if (comment == null)
            return NotFound("No comment found with the specified id");

        await _newsService.DeleteNewsCommentAsync(comment);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteNewsComment",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteNewsComment"), comment.Id), comment);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelectedComments([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return Ok(defaultMessage: true);

        var comments = await _newsService.GetNewsCommentsByIdsAsync(selectedIds.ToArray());

        await _newsService.DeleteNewsCommentsAsync(comments);

        //activity log
        foreach (var newsComment in comments)
        {
            await _customerActivityService.InsertActivityAsync("DeleteNewsComment",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteNewsComment"), newsComment.Id), newsComment);
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ApproveSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return Ok(defaultMessage: true);

        //filter not approved comments
        var newsComments = (await _newsService.GetNewsCommentsByIdsAsync(selectedIds.ToArray())).Where(comment => !comment.IsApproved);

        foreach (var newsComment in newsComments)
        {
            newsComment.IsApproved = true;

            await _newsService.UpdateNewsCommentAsync(newsComment);

            //raise event 
            await _eventPublisher.PublishAsync(new NewsCommentApprovedEvent(newsComment));

            //activity log
            await _customerActivityService.InsertActivityAsync("EditNewsComment",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditNewsComment"), newsComment.Id), newsComment);
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DisapproveSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return Ok(defaultMessage: true);

        //filter approved comments
        var newsComments = (await _newsService.GetNewsCommentsByIdsAsync(selectedIds.ToArray())).Where(comment => comment.IsApproved);

        foreach (var newsComment in newsComments)
        {
            newsComment.IsApproved = false;

            await _newsService.UpdateNewsCommentAsync(newsComment);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditNewsComment",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditNewsComment"), newsComment.Id), newsComment);
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #endregion
}