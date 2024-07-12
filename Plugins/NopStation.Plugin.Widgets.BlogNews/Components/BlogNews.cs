using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Blogs;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.News;
using Nop.Services.Seo;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widget.BlogNews.Domains;
using NopStation.Plugin.Widget.BlogNews.Infrastructure.Cache;
using NopStation.Plugin.Widget.BlogNews.Models;
using NopStation.Plugin.Widget.BlogNews.Services;

namespace NopStation.Plugin.Widget.BlogNews.Components;

public partial class BlogNewsViewComponent : NopStationViewComponent
{
    private readonly IStoreContext _storeContext;
    private readonly IBlogNewsPictureService _blogNewsPictureService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly BlogNewsSettings _blogNewsSettings;
    private readonly IStaticCacheManager _cacheManager;
    private readonly IPictureService _pictureService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IBlogService _blogService;
    private readonly INewsService _newsService;
    private readonly IWorkContext _workContext;
    private readonly IStoreMappingService _storeMappingService;

    public BlogNewsViewComponent(
        IStoreContext storeContext,
        IBlogNewsPictureService blogNewsPictureService,
        IUrlRecordService urlRecordService,
        BlogNewsSettings blogNewsSettings,
        IStaticCacheManager cacheManager,
        IPictureService pictureService,
        IDateTimeHelper dateTimeHelper,
        IBlogService blogService,
        INewsService newsService,
        IWorkContext workContext,
        IStoreMappingService storeMappingService)
    {
        _blogNewsPictureService = blogNewsPictureService;
        _blogNewsSettings = blogNewsSettings;
        _pictureService = pictureService;
        _urlRecordService = urlRecordService;
        _blogService = blogService;
        _newsService = newsService;
        _dateTimeHelper = dateTimeHelper;
        _workContext = workContext;
        _storeMappingService = storeMappingService;
        _storeContext = storeContext;
        _cacheManager = cacheManager;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        //if (!_licenseService.IsLicensed())
        //    return Content("");

        // this view was not found in the particular directory. view path added
        var model = await PreparePublicModelAsync();
        return View(model);

    }

    private async Task<PublicModel> PreparePublicModelAsync()
    {
        var newsPageSize = _blogNewsSettings.NumberOfNewsItemsToShow == 0 ? int.MaxValue :
            _blogNewsSettings.NumberOfNewsItemsToShow;
        var blogPageSize = _blogNewsSettings.NumberOfBlogPostsToShow == 0 ? int.MaxValue :
            _blogNewsSettings.NumberOfBlogPostsToShow;

        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(BlogNewsCacheEventConsumer.HOMEPAGE_BLOGNEWS_MODEL_KEY,
            await _workContext.GetWorkingLanguageAsync(),
            await _storeContext.GetCurrentStoreAsync(),
            newsPageSize,
            blogPageSize,
            _blogNewsSettings.ShowNewsInStore,
            _blogNewsSettings.NewsItemPictureSize,
            _blogNewsSettings.ShowBlogsInStore,
            _blogNewsSettings.BlogPostPictureSize);

        var defaultCacheModel = await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var model = new PublicModel();
            if (_blogNewsSettings.ShowBlogsInStore)
            {
                var blogPictures = await _blogNewsPictureService.GetAllPicturesAsync(EntityType.Blog, true, true,
                    (await _storeContext.GetCurrentStoreAsync()).Id, (await _workContext.GetWorkingLanguageAsync()).Id, 0, blogPageSize);

                if (blogPictures.Any())
                {
                    var blogs = blogPictures.SelectAwait(async x => await _blogService.GetBlogPostByIdAsync(x.EntityId));
                    foreach (var blogPicture in blogPictures)
                    {
                        var blog = await blogs.FirstOrDefaultAsync(x => x.Id == blogPicture.EntityId);

                        var picture = await _pictureService.GetPictureByIdAsync(blogPicture.PictureId);

                        var mm = new BlogPostModel()
                        {
                            AltAttribute = picture != null ? picture.AltAttribute : "",
                            TitleAttribute = picture != null ? picture.AltAttribute : "",
                            PictureUrl = picture != null ? await _pictureService.GetPictureUrlAsync(picture.Id, _blogNewsSettings.BlogPostPictureSize) :
                                await _pictureService.GetDefaultPictureUrlAsync(_blogNewsSettings.BlogPostPictureSize),
                            SeName = await _urlRecordService.GetSeNameAsync(blog, blog.LanguageId, false, false),
                            CreatedOnUtcStr = (await _dateTimeHelper.ConvertToUserTimeAsync(blog.CreatedOnUtc, DateTimeKind.Utc)).ToString("MMMM dd, yyyy"),
                            BodyOverview = blog.BodyOverview,
                            Id = blog.Id,
                            TotalComments = await _blogService.GetBlogCommentsCountAsync(blog, isApproved: true),
                            Title = blog.Title,
                            AllowComments = blog.AllowComments
                        };
                        model.BlogPosts.Add(mm);
                    }
                }
            }

            if (_blogNewsSettings.ShowNewsInStore)
            {
                var newsPictures = await _blogNewsPictureService.GetAllPicturesAsync(EntityType.News, true, true,
                    (await _storeContext.GetCurrentStoreAsync()).Id, (await _workContext.GetWorkingLanguageAsync()).Id, 0, newsPageSize);

                if (newsPictures.Any())
                {
                    //var newsItems = await _newsService.GetNewsCommentsByIdsAsync(newsPictures.Select(x => x.EntityId).ToArray());
                    var newsItems = await _newsService.GetAllNewsAsync();
                    foreach (var newsPicture in newsPictures)
                    {
                        var news = newsItems.FirstOrDefault(x => x.Id == newsPicture.EntityId);

                        var picture = await _pictureService.GetPictureByIdAsync(newsPicture.PictureId);


                        var mm = new NewsItemModel()
                        {
                            AltAttribute = picture != null ? picture.AltAttribute : "",
                            TitleAttribute = picture != null ? picture.AltAttribute : "",
                            PictureUrl = picture != null ? await _pictureService.GetPictureUrlAsync(picture.Id, _blogNewsSettings.BlogPostPictureSize) :
                                await _pictureService.GetDefaultPictureUrlAsync(_blogNewsSettings.BlogPostPictureSize),
                            SeName = await _urlRecordService.GetSeNameAsync(news, news.LanguageId, false, false),
                            CreatedOnUtcStr = (await _dateTimeHelper.ConvertToUserTimeAsync(news.CreatedOnUtc, DateTimeKind.Utc)).ToString("MMMM dd, yyyy"),
                            ShortDescription = news.Short,
                            Id = news.Id,
                            TotalComments = await _newsService.GetNewsCommentsCountAsync(news, isApproved: true),
                            Title = news.Title,
                            AllowComments = news.AllowComments
                        };
                        model.NewsItems.Add(mm);
                    }
                }
            }
            return model;
        });

        return defaultCacheModel;
    }
}
