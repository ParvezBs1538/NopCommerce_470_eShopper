using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Shipping.VendorAndState.Domain;
using NopStation.Plugin.Shipping.VendorAndState.Services.Cache;

namespace NopStation.Plugin.Shipping.VendorAndState.Services
{
    public class VendorStateProvinceShippingService : IVendorStateProvinceShippingService
    {
        private readonly IStaticCacheManager _cacheManager;
        private readonly IRepository<VendorStateProvinceShipping> _vendorStateProvinceShippingRepository;

        public VendorStateProvinceShippingService(IStaticCacheManager cacheManager,
            IRepository<VendorStateProvinceShipping> vendorStateProvinceShippingRepository)
        {
            _cacheManager = cacheManager;
            _vendorStateProvinceShippingRepository = vendorStateProvinceShippingRepository;
        }

        public async Task<VendorStateProvinceShipping> GetVendorStateProvinceShippingByIdAsync(int id)
        {
            return await _vendorStateProvinceShippingRepository.GetByIdAsync(id);
        }

        public async Task<VendorStateProvinceShipping> GetVendorStateProvinceShippingByVendorIdAndShippingMethodIdAsync(int vendorId, int shippingMethodId, int stateProvinceId)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ShippingByVendorCacheDefaults.VendorStateProvinceShippingVendorAndShippingIdKey, vendorId, shippingMethodId, stateProvinceId);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from vs in _vendorStateProvinceShippingRepository.Table
                            where vs.VendorId == vendorId &&
                            vs.ShippingMethodId == shippingMethodId &&
                            vs.StateProvinceId == stateProvinceId
                            orderby vs.VendorId descending
                            select vs;

                return await query.FirstOrDefaultAsync();
            });
        }

        public async Task InsertVendorStateProvinceShippingAsync(VendorStateProvinceShipping vendorStateProvinceShipping)
        {
            await _vendorStateProvinceShippingRepository.InsertAsync(vendorStateProvinceShipping);
        }

        public async Task UpdateVendorStateProvinceShippingAsync(VendorStateProvinceShipping vendorStateProvinceShipping)
        {
            await _vendorStateProvinceShippingRepository.UpdateAsync(vendorStateProvinceShipping);
        }

        public async Task DeleteVendorStateProvinceShippingAsync(VendorStateProvinceShipping vendorStateProvinceShipping)
        {
            await _vendorStateProvinceShippingRepository.DeleteAsync(vendorStateProvinceShipping);
        }

        public async Task<IPagedList<VendorStateProvinceShipping>> GetAllVendorStateProvinceShippingsAsync(int shippingMethodId = 0, int vendorId = 0, int stateProvinceId = 0,
            bool? hideShippingMethod = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ShippingByVendorCacheDefaults.VendorStateProvinceShippingAllKey, vendorId, shippingMethodId, stateProvinceId,
                hideShippingMethod, pageIndex, pageSize);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from vsps in _vendorStateProvinceShippingRepository.Table
                            where (stateProvinceId == 0 || vsps.StateProvinceId == stateProvinceId) &&
                            (shippingMethodId == 0 || vsps.ShippingMethodId == shippingMethodId) &&
                            (!hideShippingMethod.HasValue || vsps.HideShippingMethod == hideShippingMethod.Value) &&
                            (vendorId == 0 || vsps.VendorId == vendorId)
                            select vsps;

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }
    }
}
