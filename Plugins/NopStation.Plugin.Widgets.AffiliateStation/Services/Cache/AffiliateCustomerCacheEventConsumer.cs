using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services.Cache
{
    public partial class AffiliateCustomerCacheEventConsumer : CacheEventConsumer<AffiliateCustomer>
    {
        protected override async Task ClearCacheAsync(AffiliateCustomer entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<AffiliateCustomer>.Prefix);
            await RemoveByPrefixAsync(AffiliateStationCacheDefaults.AffiliateCustomerPrefix);
        }
    }
}