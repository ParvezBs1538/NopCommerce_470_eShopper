using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using NopStation.Plugin.Shipping.VendorAndState.Domain;
using NopStation.Plugin.Shipping.VendorAndState.Services.Cache;

namespace NopStation.Plugin.Shipping.VendorAndState.Services
{
    public class VendorShippingService : IVendorShippingService
    {
        private readonly IStaticCacheManager _cacheManager;
        private readonly IRepository<VendorShipping> _vendorShippingRepository;

        public VendorShippingService(IStaticCacheManager cacheManager,
            IRepository<VendorShipping> vendorShippingRepository)
        {
            _cacheManager = cacheManager;
            _vendorShippingRepository = vendorShippingRepository;
        }

        public async Task<VendorShipping> GetVendorShippingByIdAsync(int id)
        {
            return await _vendorShippingRepository.GetByIdAsync(id);
        }

        public async Task<VendorShipping> GetVendorShippingByVendorIdAndShippingMethodIdAsync(int vendorId, int shippingMethodId)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ShippingByVendorCacheDefaults.VendorShippingByVendorAndShippingIdKey, vendorId, shippingMethodId);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from vs in _vendorShippingRepository.Table
                            where vs.VendorId == vendorId &&
                            vs.ShippingMethodId == shippingMethodId
                            orderby vs.VendorId descending
                            select vs;

                return await query.FirstOrDefaultAsync();
            });
        }

        public async Task InsertVendorShippingAsync(VendorShipping vendorShipping)
        {
            await _vendorShippingRepository.InsertAsync(vendorShipping);
        }

        public async Task UpdateVendorShippingAsync(VendorShipping vendorShipping)
        {
            await _vendorShippingRepository.UpdateAsync(vendorShipping);
        }

        public async Task DeleteVendorShippingAsync(VendorShipping vendorShipping)
        {
            await _vendorShippingRepository.DeleteAsync(vendorShipping);
        }

        public async Task<IPagedList<VendorShipping>> GetAllVendorShippingsAsync(int shippingMethodId = 0, int vendorId = 0, 
            bool? hideShippingMethod = null, bool? sellerSideDelivery = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ShippingByVendorCacheDefaults.VendorShippingsAllKey, shippingMethodId, vendorId,
                hideShippingMethod, sellerSideDelivery, pageIndex, pageSize);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from vs in _vendorShippingRepository.Table
                            where (vendorId == 0 || vs.VendorId == vendorId) &&
                            (shippingMethodId == 0 || vs.ShippingMethodId == shippingMethodId) &&
                            (!hideShippingMethod.HasValue || vs.HideShippingMethod == hideShippingMethod.Value) &&
                            (!sellerSideDelivery.HasValue || vs.SellerSideDelivery == sellerSideDelivery.Value)
                            select vs;

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }
    }
}
