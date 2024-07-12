using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Data;
using NopStation.Plugin.Widgets.Product360View.Domain;
using NopStation.Plugin.Widgets.Product360View.Infrastructure;

namespace NopStation.Plugin.Widgets.Product360View.Services
{
    public class ProductPictureMappingService : IProductPictureMappingService
    {
        #region Properties

        private readonly IRepository<ProductPictureMapping360> _pictureMappingRepository;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region  Ctors

        public ProductPictureMappingService(IRepository<ProductPictureMapping360> pictureMappingRepository,
            IRepository<Picture> pictureRepository,
            IStaticCacheManager staticCacheManager)
        {
            _pictureMappingRepository = pictureMappingRepository;
            _pictureRepository = pictureRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region  Methods

        public virtual async Task<ProductPictureMapping360> GetPictureMappingByIdAsync(int id)
        {
            var query = _pictureMappingRepository.Table;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(Picture360CacheKeys.PictureMappingCacheKey, id);

            var cachedSetting = await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync(c => c.Id == id));

            return cachedSetting;
        }

        public virtual async Task InsertPictureMappingAsync(ProductPictureMapping360 pictureMapping)
        {
            await _pictureMappingRepository.InsertAsync(pictureMapping);
            await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.PictureMapping360Prefix, pictureMapping.Id);
            await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.Picture360Prefix, pictureMapping.ProductId);
        }
        public virtual async Task UpdatePictureMappingAsync(ProductPictureMapping360 pictureMapping)
        {
            await _pictureMappingRepository.UpdateAsync(pictureMapping);

            await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.PictureMapping360Prefix, pictureMapping.Id);
            await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.Picture360Prefix, pictureMapping.ProductId);
        }

        public virtual async Task DeletePictureMappingAsync(ProductPictureMapping360 pictureMapping)
        {
            await _pictureMappingRepository.DeleteAsync(pictureMapping);

            await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.PictureMapping360Prefix, pictureMapping.Id);
            await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.Picture360Prefix, pictureMapping.ProductId);
        }

        public virtual async Task<List<ProductPictureMapping360>> GetPictureMappingsByProductIdAsync(int productId, bool isPanorama = false)
        {
            var query = from pp in _pictureMappingRepository.Table
                        where pp.ProductId == productId && pp.IsPanorama == isPanorama
                        orderby pp.DisplayOrder, pp.Id
                        select pp;

            return await query.ToListAsync();
        }

        public virtual async Task<IList<Picture>> Get360PicturesByProductIdAsync(int productId, bool isPanorama = false)
        {
            if (productId == 0)
                return new List<Picture>();

            var pictureQuery = from p in _pictureRepository.Table
                               join pp in _pictureMappingRepository.Table on p.Id equals pp.PictureId
                               orderby pp.DisplayOrder, pp.Id
                               where pp.ProductId == productId && pp.IsPanorama == isPanorama
                               select p;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(Picture360CacheKeys.PicturesCacheKey, productId, isPanorama);
            var cachedPictures = await _staticCacheManager.GetAsync(key, async () => await pictureQuery.ToListAsync());

            return cachedPictures;
        }

        public virtual async Task<int> GetLastPictureOrderByProductIdAsync(int productId, bool isPanorama = false)
        {
            if (productId > 0)
            {
                var query = from p in _pictureRepository.Table
                            join pp in _pictureMappingRepository.Table on p.Id equals pp.PictureId
                            orderby pp.DisplayOrder descending
                            where pp.ProductId == productId && pp.IsPanorama == isPanorama
                            select pp;

                var lastPicture = await query.FirstOrDefaultAsync();
                if (lastPicture != null)
                    return lastPicture.DisplayOrder;
            }
            return -1;
        }

        #endregion
    }
}
