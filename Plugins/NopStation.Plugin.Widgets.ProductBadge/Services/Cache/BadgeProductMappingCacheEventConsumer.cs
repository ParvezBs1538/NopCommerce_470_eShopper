using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Services.Cache;

public partial class BadgeProductMappingCacheEventConsumer : CacheEventConsumer<BadgeProductMapping>
{
    protected override async Task ClearCacheAsync(BadgeProductMapping entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.BadgeProductsPrefix, entity.BadgeId);
        await RemoveByPrefixAsync(CacheDefaults.BadgeVendorsAllPrefix);
    }
}