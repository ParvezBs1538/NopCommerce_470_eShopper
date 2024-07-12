using System.Threading.Tasks;
using NopStation.Plugin.Shipping.VendorAndState.Domain;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Services.Caching;

namespace NopStation.Plugin.Shipping.VendorAndState.Services.Cache
{
    public partial class VendorStateProvinceShippingCacheEventConsumer : CacheEventConsumer<VendorStateProvinceShipping>
    {
        protected override async Task ClearCacheAsync(VendorStateProvinceShipping entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<VendorStateProvinceShipping>.Prefix);
            await RemoveByPrefixAsync(ShippingByVendorCacheDefaults.VendorStateProvinceShippingPrefixKey);
        }
    }
}