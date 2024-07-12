using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Widgets.CategoryBanners.Domains;
using NopStation.Plugin.Widgets.CategoryBanners.Services.Cache;

namespace NopStation.Plugin.Widgets.CategoryBanners.Services
{
    public class CategoryBannerService : ICategoryBannerService
    {
        #region Fields

        private readonly IRepository<CategoryBanner> _categoryBannerRepository;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public CategoryBannerService(IRepository<CategoryBanner> categoryBannerRepository,
            IStaticCacheManager cacheManager)
        {
            _categoryBannerRepository = categoryBannerRepository;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public async Task DeleteCategoryBannerAsync(CategoryBanner categoryBanner)
        {
            await _categoryBannerRepository.DeleteAsync(categoryBanner);
            await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CategoryBannerPicturePrefixCacheKey);
        }

        public async Task InsertCategoryBannerAsync(CategoryBanner categoryBanner)
        {
            await _categoryBannerRepository.InsertAsync(categoryBanner);
            await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CategoryBannerPicturePrefixCacheKey);
        }

        public async Task UpdateCategoryBannerAsync(CategoryBanner categoryBanner)
        {
           await _categoryBannerRepository.UpdateAsync(categoryBanner);
           await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CategoryBannerPicturePrefixCacheKey);
        }

        public async Task<CategoryBanner> GetCategoryBannerByIdAsync(int categoryBannerId)
        {
            if (categoryBannerId == 0)
                return null;

            return await _categoryBannerRepository.GetByIdAsync(categoryBannerId, cache => default);
        }

        public IList<CategoryBanner> GetCategoryBannersByCategoryId(int categoryId, bool? mobileDevice = null)
        {
            var query = _categoryBannerRepository.Table.Where(x => x.CategoryId == categoryId);

            if(mobileDevice.HasValue)
                query = query.Where(x => x.ForMobile == mobileDevice.Value);

            query = query.OrderBy(e => e.DisplayOrder);

            return query.ToList();
        }

        #endregion
    }
}
