using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Blogs;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widget.BlogNews.Areas.Admin.Models;
using NopStation.Plugin.Widget.BlogNews.Domains;
using NopStation.Plugin.Widget.BlogNews.Infrastructure.Cache;
using NopStation.Plugin.Widget.BlogNews.Services;

namespace NopStation.Plugin.Widget.BlogNews.Areas.Admin.Controllers;


public class BlogNewsController : NopStationAdminController
{
    #region Fields

    private readonly INewsService _newsService;
    private readonly IBlogNewsPictureService _blogNewsPictureService;
    private readonly IPermissionService _permissionService;
    private readonly BlogNewsSettings _blogNewsSettings;
    private readonly IStaticCacheManager _cacheManager;
    private readonly IPictureService _pictureService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IBlogService _blogService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;

    #endregion

    #region Ctor

    public BlogNewsController(IBlogService blogService,
        IBlogNewsPictureService blogNewsPictureService,
        IPermissionService permissionService,
        BlogNewsSettings blogNewsSettings,
        IStaticCacheManager cacheManager,
        IPictureService pictureService,
        ISettingService settingService,
        IStoreContext storeContext,
        INewsService newsService,
        ILocalizationService localizationService,
        INotificationService notificationService)
    {
        _newsService = newsService;
        _blogNewsPictureService = blogNewsPictureService;
        _permissionService = permissionService;
        _blogNewsSettings = blogNewsSettings;
        _pictureService = pictureService;
        _settingService = settingService;
        _cacheManager = cacheManager;
        _storeContext = storeContext;
        _blogService = blogService;
        _localizationService = localizationService;
        _notificationService = notificationService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(BlogNewsPermissionProvider.ManageBlogNews))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var blogNewsSettings = await _settingService.LoadSettingAsync<BlogNewsSettings>(storeId);
        var model = blogNewsSettings.ToSettingsModel<ConfigurationModel>();

        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId <= 0)
            return View(model);

        model.WidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(blogNewsSettings, x => x.WidgetZone, storeId);
        model.BlogPostPictureSize_OverrideForStore = await _settingService.SettingExistsAsync(blogNewsSettings, x => x.BlogPostPictureSize, storeId);
        model.NewsItemPictureSize_OverrideForStore = await _settingService.SettingExistsAsync(blogNewsSettings, x => x.NewsItemPictureSize, storeId);
        model.ShowBlogsInStore_OverrideForStore = await _settingService.SettingExistsAsync(blogNewsSettings, x => x.ShowBlogsInStore, storeId);
        model.NumberOfBlogPostsToShow_OverrideForStore = await _settingService.SettingExistsAsync(blogNewsSettings, x => x.NumberOfBlogPostsToShow, storeId);
        model.ShowNewsInStore_OverrideForStore = await _settingService.SettingExistsAsync(blogNewsSettings, x => x.ShowNewsInStore, storeId);
        model.NumberOfNewsItemsToShow_OverrideForStore = await _settingService.SettingExistsAsync(blogNewsSettings, x => x.NumberOfNewsItemsToShow, storeId);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(BlogNewsPermissionProvider.ManageBlogNews))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var blogNewsSettings = await _settingService.LoadSettingAsync<BlogNewsSettings>(storeScope);
        blogNewsSettings = model.ToSettings(blogNewsSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(blogNewsSettings, x => x.WidgetZone, model.WidgetZone_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(blogNewsSettings, x => x.ShowBlogsInStore, model.ShowBlogsInStore_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(blogNewsSettings, x => x.BlogPostPictureSize, model.BlogPostPictureSize_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(blogNewsSettings, x => x.NumberOfBlogPostsToShow, model.NumberOfBlogPostsToShow_OverrideForStore, storeScope, false);

        await _settingService.SaveSettingOverridablePerStoreAsync(blogNewsSettings, x => x.ShowNewsInStore, model.ShowNewsInStore_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(blogNewsSettings, x => x.NewsItemPictureSize, model.NewsItemPictureSize_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(blogNewsSettings, x => x.NumberOfNewsItemsToShow, model.NumberOfNewsItemsToShow_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        await _cacheManager.RemoveByPrefixAsync(BlogNewsCacheEventConsumer.HOMEPAGE_BLOGNEWS_PATTERN_KEY);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.BlogNews.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    public async Task<IActionResult> BlogPostPictureSave(int pictureId, string overrideAltAttribute,
        string overrideTitleAttribute, int blogPostId, bool showOnHomePage)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AccessDeniedView();

        if (pictureId == 0)
            throw new ArgumentException();

        //try to get a product with the specified id
        var blogPost = await _blogService.GetBlogPostByIdAsync(blogPostId)
            ?? throw new ArgumentException("No blog found with the specified id");

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(pictureId)
            ?? throw new ArgumentException("No picture found with the specified id");

        await _pictureService.UpdatePictureAsync(picture.Id,
            await _pictureService.LoadPictureBinaryAsync(picture),
            picture.MimeType,
            picture.SeoFilename,
            overrideAltAttribute,
            overrideTitleAttribute);

        await _pictureService.SetSeoFilenameAsync(pictureId, await _pictureService.GetPictureSeNameAsync(blogPost.Title));

        var blogNewsPicture = _blogNewsPictureService.GetBlogNewsPictureByEntytiId(blogPostId, EntityType.Blog);
        if (blogNewsPicture != null)
        {
            if (blogNewsPicture.PictureId != pictureId)
            {
                var oldPicture = await _pictureService.GetPictureByIdAsync(blogNewsPicture.PictureId);
                if (oldPicture != null)
                    await _pictureService.DeletePictureAsync(oldPicture);
            }

            blogNewsPicture.PictureId = pictureId;
            blogNewsPicture.ShowInStore = showOnHomePage;
            await _blogNewsPictureService.UpdateBlogNewsPictureAsync(blogNewsPicture);
        }
        else
        {
            var newBlogNewsPicture = new BlogNewsPicture()
            {
                EntityId = blogPostId,
                PictureId = pictureId,
                EntityType = EntityType.Blog,
                ShowInStore = showOnHomePage
            };
            await _blogNewsPictureService.InsertBlogNewsPictureAsync(newBlogNewsPicture);
        }

        return Json(new { Result = true, PictureUrl = await _pictureService.GetPictureUrlAsync(picture.Id, _blogNewsSettings.BlogPostPictureSize) });
    }

    public async Task<IActionResult> NewsItemPictureSave(int pictureId, string overrideAltAttribute,
        string overrideTitleAttribute, int newsItemId, bool showOnHomePage)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
            return AccessDeniedView();

        if (pictureId == 0)
            throw new ArgumentException();

        //try to get a product with the specified id
        var blogPost = await _newsService.GetNewsByIdAsync(newsItemId)
            ?? throw new ArgumentException("No news found with the specified id");

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(pictureId)
            ?? throw new ArgumentException("No picture found with the specified id");

        await _pictureService.UpdatePictureAsync(picture.Id,
            await _pictureService.LoadPictureBinaryAsync(picture),
            picture.MimeType,
            picture.SeoFilename,
            overrideAltAttribute,
            overrideTitleAttribute);

        await _pictureService.SetSeoFilenameAsync(pictureId, await _pictureService.GetPictureSeNameAsync(blogPost.Title));

        var blogNewsPicture = _blogNewsPictureService.GetBlogNewsPictureByEntytiId(newsItemId, EntityType.News);
        if (blogNewsPicture != null)
        {
            if (blogNewsPicture.PictureId != pictureId)
            {
                var oldPicture = await _pictureService.GetPictureByIdAsync(blogNewsPicture.PictureId);
                if (oldPicture != null)
                    await _pictureService.DeletePictureAsync(oldPicture);
            }

            blogNewsPicture.PictureId = pictureId;
            blogNewsPicture.ShowInStore = showOnHomePage;
            await _blogNewsPictureService.UpdateBlogNewsPictureAsync(blogNewsPicture);
        }
        else
        {
            var newBlogNewsPicture = new BlogNewsPicture()
            {
                EntityId = newsItemId,
                PictureId = pictureId,
                EntityType = EntityType.News,
                ShowInStore = showOnHomePage
            };
            await _blogNewsPictureService.InsertBlogNewsPictureAsync(newBlogNewsPicture);
        }

        return Json(new { Result = true, PictureUrl = await _pictureService.GetPictureUrlAsync(picture.Id, _blogNewsSettings.NewsItemPictureSize) });
    }

    #endregion
}
