using System.Threading.Tasks;
using NopStation.Plugin.Shipping.VendorAndState.Domain;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Services.Caching;

namespace NopStation.Plugin.Shipping.VendorAndState.Services.Cache
{
    public partial class VendorShippingCacheEventConsumer : CacheEventConsumer<VendorShipping>
    {
        protected override async Task ClearCacheAsync(VendorShipping entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<VendorShipping>.Prefix);
            await RemoveByPrefixAsync(ShippingByVendorCacheDefaults.VendorShippingPrefix);
        }
    }
}