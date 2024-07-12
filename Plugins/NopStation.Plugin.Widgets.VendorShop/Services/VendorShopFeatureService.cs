using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Widgets.VendorShop.Domains;
using NopStation.Plugin.Widgets.VendorShop.Services.Cache;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public class VendorShopFeatureService : IVendorShopFeatureService
    {
        private readonly IRepository<VendorFeatureMapping> _repository;
        private readonly IStaticCacheManager _staticCacheManager;

        public VendorShopFeatureService(IRepository<VendorFeatureMapping> repository,
            IStaticCacheManager staticCacheManager)
        {
            _repository = repository;
            _staticCacheManager = staticCacheManager;
        }
        public async Task DeleteAsync(VendorFeatureMapping vendorFeatureMapping)
        {
            await _repository.DeleteAsync(vendorFeatureMapping);
        }
        public async Task<VendorFeatureMapping> GetVendorFeatureMappingByIdAsync(int id)
        {
            if (id == 0)
                return null;
            else
                return await _repository.GetByIdAsync(id);

        }

        public async Task<VendorFeatureMapping> GetVendorFeatureMappingByVendorIdAsync(int vendorId)
        {
            if (vendorId == 0)
                return null;

            return await _repository.Table.Where(x => x.VendorId == vendorId).FirstOrDefaultAsync();
        }
        public async Task<bool> IsEnableVendorShopAsync(int vendorId)
        {
            var cacheKey = VendorShopFeatureCacheDefault.GetVendorShopIsEnabledCacheKey(vendorId);
            var result = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var vendorFeatureMapping = await GetVendorFeatureMappingByVendorIdAsync(vendorId);
                if (vendorFeatureMapping == null)
                    return false;
                else
                    return vendorFeatureMapping.Enable;
            });

            return result;
        }

        public async Task InsertAsync(VendorFeatureMapping vendorFeatureMapping)
        {
            await _repository.InsertAsync(vendorFeatureMapping);
        }
        public async Task UpdateAsync(VendorFeatureMapping vendorFeatureMapping)
        {
            await _repository.UpdateAsync(vendorFeatureMapping);
        }
    }
}
