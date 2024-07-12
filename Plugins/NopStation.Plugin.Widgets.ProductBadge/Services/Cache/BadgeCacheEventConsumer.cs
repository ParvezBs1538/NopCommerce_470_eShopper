using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Services.Cache;

public partial class BadgeCacheEventConsumer : CacheEventConsumer<Badge>
{
    protected override async Task ClearCacheAsync(Badge entity)
    {
        await RemoveAsync(CacheDefaults.BadgeBestSellingProductKey);
        await RemoveByPrefixAsync(CacheDefaults.ActiveBadgePrefix);
        await RemoveByPrefixAsync(CacheDefaults.BadgeModelPrefix, entity.Id);
    }
}