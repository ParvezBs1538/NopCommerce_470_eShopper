using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services.Cache
{
    public partial class OrderCommissionCacheEventConsumer : CacheEventConsumer<OrderCommission>
    {
        protected override async Task ClearCacheAsync(OrderCommission entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<OrderCommission>.Prefix);
            await RemoveByPrefixAsync(AffiliateStationCacheDefaults.OrderCommissionPrefix);
        }
    }
}