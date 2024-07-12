using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.News;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widget.BlogNews.Domains;

namespace NopStation.Plugin.Widget.BlogNews.Infrastructure.Cache;

public class BlogNewsCacheEventConsumer : IConsumer<EntityInsertedEvent<BlogNewsPicture>>,
    IConsumer<EntityUpdatedEvent<BlogNewsPicture>>,
    IConsumer<EntityDeletedEvent<BlogNewsPicture>>,

    IConsumer<EntityInsertedEvent<BlogPost>>,
    IConsumer<EntityUpdatedEvent<BlogPost>>,
    IConsumer<EntityDeletedEvent<BlogPost>>,

    IConsumer<EntityInsertedEvent<NewsItem>>,
    IConsumer<EntityUpdatedEvent<NewsItem>>,
    IConsumer<EntityDeletedEvent<NewsItem>>
{
    #region Fields

    private readonly BlogNewsSettings _blogNewsSettings;
    private readonly IStaticCacheManager _cacheManager;

    #endregion

    #region Ctor

    public BlogNewsCacheEventConsumer(BlogNewsSettings blogNewsSettings, IStaticCacheManager cacheManager)
    {
        this._cacheManager = cacheManager;
        this._blogNewsSettings = blogNewsSettings;
    }

    #endregion

    #region Cache keys 

    public static CacheKey HOMEPAGE_BLOGNEWS_MODEL_KEY = new CacheKey("NopStation.blognews.homepage-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}", HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    public const string HOMEPAGE_BLOGNEWS_PATTERN_KEY = "NopStation.blognews.homepage";

    #endregion

    #region Methods

    public async Task HandleEventAsync(EntityInsertedEvent<BlogNewsPicture> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<BlogNewsPicture> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<BlogNewsPicture> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<BlogPost> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<BlogPost> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<BlogPost> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<NewsItem> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<NewsItem> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<NewsItem> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(HOMEPAGE_BLOGNEWS_PATTERN_KEY);
    }

    #endregion
}
