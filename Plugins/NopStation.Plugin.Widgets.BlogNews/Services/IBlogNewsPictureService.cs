using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widget.BlogNews.Domains;

namespace NopStation.Plugin.Widget.BlogNews.Services;

public interface IBlogNewsPictureService
{
    Task InsertBlogNewsPictureAsync(BlogNewsPicture blogNewsPicture);

    Task UpdateBlogNewsPictureAsync(BlogNewsPicture blogNewsPicture);

    Task DeleteBlogNewsPictureAsync(BlogNewsPicture blogNewsPicture);

    BlogNewsPicture GetBlogNewsPictureByEntytiId(int entityId, EntityType entityType);

    Task<IPagedList<BlogNewsPicture>> GetAllPicturesAsync(EntityType entityType,
        bool? showOnHomePage = null, bool? published = null, int storeId = 0,
        int languageId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
}