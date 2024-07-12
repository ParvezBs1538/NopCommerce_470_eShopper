using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.CategoryBanners.Domains;

namespace NopStation.Plugin.Widgets.CategoryBanners.Services
{
    public interface ICategoryBannerService
    {
        Task DeleteCategoryBannerAsync(CategoryBanner categoryBanner);

        Task InsertCategoryBannerAsync(CategoryBanner categoryBanner);

        Task UpdateCategoryBannerAsync(CategoryBanner categoryBanner);

        Task<CategoryBanner> GetCategoryBannerByIdAsync(int categoryBannerId);

        IList<CategoryBanner> GetCategoryBannersByCategoryId(int categoryId, bool? mobileDevice = null);
    }
}