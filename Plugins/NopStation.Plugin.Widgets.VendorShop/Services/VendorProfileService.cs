using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using NopStation.Plugin.Widgets.VendorShop.Domains;
using NopStation.Plugin.Widgets.VendorShop.Infrastructure.Cache;
using NopStation.Plugin.Widgets.VendorShop.Models.ProfileTabs;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public class VendorProfileService : IVendorProfileService
    {
        private readonly IRepository<VendorProfile> _vendorProfileRepository;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;

        public VendorProfileService(IRepository<VendorProfile> vendorProfileRepository,
            IStaticCacheManager cacheManager,
            IRepository<Product> productRepository,
            IRepository<ProductReview> productReviewRepository)
        {
            _vendorProfileRepository = vendorProfileRepository;
            _cacheManager = cacheManager;
            _productRepository = productRepository;
            _productReviewRepository = productReviewRepository;
        }
        public async Task<VendorProfile> GetVendorProfileAsync(int vendorId, int storeId = 0)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(VendorProfileCacheDefaults.GetVendorProfileCacheKey(vendorId), storeId);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                return await _vendorProfileRepository.Table.Where(vp => vp.VendorId == vendorId && (storeId == 0 || vp.StoreId == storeId)).FirstOrDefaultAsync();
            });
        }

        public async Task SaveVendorProfileAsync(VendorProfile vendorProfile)
        {
            if (vendorProfile.Id == 0)
            {
                await _vendorProfileRepository.InsertAsync(vendorProfile);
                return;
            }

            await _vendorProfileRepository.UpdateAsync(vendorProfile);

        }

        public async Task<IPagedList<ProductReview>> GetVendorProductReviewsAsync(int vendorId, int filterBy = 0, int orderBy = 0, int page = 1, int pageSize = int.MaxValue)
        {
            var query = from product in _productRepository.Table
                        join review in _productReviewRepository.Table on product.Id equals review.ProductId
                        where product.VendorId == vendorId && review.IsApproved
                        select review;

            if (filterBy > 0)
            {
                query = query.Where(review => review.Rating == filterBy);
            }
            if (orderBy == (int)VendorReviewsSortingEnum.Recent)
            {
                query = query
                .OrderByDescending(review => review.CreatedOnUtc);
            }
            else if (orderBy == (int)VendorReviewsSortingEnum.RatingHighToLow)
            {
                query = query
                .OrderByDescending(review => review.Rating)
                .ThenByDescending(review => review.CreatedOnUtc);
            }
            else if (orderBy == (int)VendorReviewsSortingEnum.RatingLowToHigh)
            {
                query = query
                .OrderBy(review => review.Rating)
                .ThenBy(review => review.CreatedOnUtc);
            }
            return await query.ToPagedListAsync(page - 1, pageSize);
        }
    }
}
