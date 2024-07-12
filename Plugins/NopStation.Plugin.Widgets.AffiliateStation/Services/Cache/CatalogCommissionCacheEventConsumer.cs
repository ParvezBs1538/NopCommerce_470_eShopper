using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services.Cache
{
    public partial class CatalogCommissionCacheEventConsumer : CacheEventConsumer<CatalogCommission>
    {
        protected override async Task ClearCacheAsync(CatalogCommission entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<CatalogCommission>.Prefix);
            await RemoveByPrefixAsync(AffiliateStationCacheDefaults.CatalogCommissionPrefix);
        }
    }
}