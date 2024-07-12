using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Services.Cache;

public partial class BadgeCategoryMappingCacheEventConsumer : CacheEventConsumer<BadgeCategoryMapping>
{
    protected override async Task ClearCacheAsync(BadgeCategoryMapping entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.BadgeCategoriesPrefix, entity.BadgeId);
    }
}