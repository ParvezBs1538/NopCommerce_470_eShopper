using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Data;
using NopStation.Plugin.Widget.BlogNews.Domains;

namespace NopStation.Plugin.Widget.BlogNews.Services;

public class BlogNewsPictureService : IBlogNewsPictureService
{
    #region Fields


    private readonly IRepository<BlogNewsPicture> _blogNewsPictureRepository;
    private readonly IRepository<BlogPost> _blogPostRepository;
    private readonly IRepository<NewsItem> _newsItemRepository;
    private readonly IRepository<StoreMapping> _storeMappingRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public BlogNewsPictureService(
        IRepository<BlogNewsPicture> blogNewsPictureRepository,
        IRepository<BlogPost> blogPostRepository,
        IRepository<NewsItem> newsItemRepository,
        IRepository<StoreMapping> storeMappingRepository,
        IEventPublisher eventPublisher,
        INopDataProvider nopDataProvider
        )
    {
        _blogNewsPictureRepository = blogNewsPictureRepository;
        _blogPostRepository = blogPostRepository;
        _newsItemRepository = newsItemRepository;
        _storeMappingRepository = storeMappingRepository;
        _eventPublisher = eventPublisher;
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    public async Task InsertBlogNewsPictureAsync(BlogNewsPicture blogNewsPicture)
    {
        await _blogNewsPictureRepository.InsertAsync(blogNewsPicture);

        await _eventPublisher.EntityInsertedAsync(blogNewsPicture);
    }

    public async Task UpdateBlogNewsPictureAsync(BlogNewsPicture blogNewsPicture)
    {
        await _blogNewsPictureRepository.UpdateAsync(blogNewsPicture);

        await _eventPublisher.EntityUpdatedAsync(blogNewsPicture);
    }

    public async Task DeleteBlogNewsPictureAsync(BlogNewsPicture blogNewsPicture)
    {
        await _blogNewsPictureRepository.DeleteAsync(blogNewsPicture);

        await _eventPublisher.EntityDeletedAsync(blogNewsPicture);
    }

    public BlogNewsPicture GetBlogNewsPictureByEntytiId(int entityId, EntityType entityType)
    {
        return _blogNewsPictureRepository.Table
            .FirstOrDefault(x => x.EntityId == entityId &&
            x.EntityTypeId == (int)entityType);
    }

    public async Task<IPagedList<BlogNewsPicture>> GetAllPicturesAsync(EntityType entityType,
        bool? showOnHomePage = null, bool? published = null, int storeId = 0,
        int languageId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        if (entityType.Equals(EntityType.Blog))
        {
            var bnpPostquery = from bnp in _blogNewsPictureRepository.Table
                               join bp in _blogPostRepository.Table on bnp.EntityId equals bp.Id
                               where bnp.EntityTypeId == (int)entityType
                               where bp.StartDateUtc == null || bp.StartDateUtc <= DateTime.UtcNow
                               where bp.EndDateUtc == null || bp.EndDateUtc >= DateTime.UtcNow
                               select new { blogNewsPost = bnp, blogPost = bp };

            if (showOnHomePage.HasValue)
                bnpPostquery = from bnp in bnpPostquery
                               where bnp.blogNewsPost.ShowInStore == showOnHomePage
                               select bnp;

            if (languageId > 0)
                bnpPostquery = from bnp in bnpPostquery
                               where bnp.blogPost.LanguageId == languageId
                               select bnp;

            if (storeId > 0)
                bnpPostquery = from bnp in bnpPostquery
                               where !bnp.blogPost.LimitedToStores ||
                                     _storeMappingRepository.Table.Any(sm =>
                                       sm.EntityName == typeof(BlogPost).Name &&
                                       sm.EntityId == bnp.blogPost.Id &&
                                       sm.StoreId == storeId)
                               select bnp;

            bnpPostquery = bnpPostquery.OrderByDescending(bnp => bnp.blogPost.Id);

            return await bnpPostquery.Select(x => x.blogNewsPost).ToPagedListAsync(pageIndex, pageSize);
        }
        else
        {
            var bnpNewsquery = from bnp in _blogNewsPictureRepository.Table
                               join ni in _newsItemRepository.Table on bnp.EntityId equals ni.Id
                               where ni.Published
                               where bnp.EntityTypeId == (int)entityType
                               where ni.StartDateUtc == null || ni.StartDateUtc <= DateTime.UtcNow
                               where ni.EndDateUtc == null || ni.EndDateUtc >= DateTime.UtcNow
                               select new { blogNewsPost = bnp, newsItem = ni };

            if (published.HasValue)
                bnpNewsquery = from bnp in bnpNewsquery
                               where bnp.newsItem.Published == published
                               select bnp;

            if (showOnHomePage.HasValue)
                bnpNewsquery = from bnp in bnpNewsquery
                               where bnp.blogNewsPost.ShowInStore == showOnHomePage
                               select bnp;

            if (languageId > 0)
                bnpNewsquery = from bnp in bnpNewsquery
                               where bnp.newsItem.LanguageId == languageId
                               select bnp;

            if (storeId > 0)
                bnpNewsquery = from bnp in bnpNewsquery
                               where !bnp.newsItem.LimitedToStores ||
                                     _storeMappingRepository.Table.Any(sm =>
                                       sm.EntityName == typeof(NewsItem).Name &&
                                       sm.EntityId == bnp.newsItem.Id &&
                                       sm.StoreId == storeId)
                               select bnp;

            bnpNewsquery = bnpNewsquery.OrderByDescending(bnp => bnp.newsItem.Id);

            return await bnpNewsquery.Select(x => x.blogNewsPost).ToPagedListAsync(pageIndex, pageSize);
        }
    }

    #endregion
}
