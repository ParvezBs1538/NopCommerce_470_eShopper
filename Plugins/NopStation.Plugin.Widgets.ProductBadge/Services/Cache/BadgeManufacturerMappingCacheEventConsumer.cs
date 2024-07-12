using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Services.Cache;

public partial class BadgeManufacturerMappingCacheEventConsumer : CacheEventConsumer<BadgeManufacturerMapping>
{
    protected override async Task ClearCacheAsync(BadgeManufacturerMapping entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.BadgeManufacturersPrefix, entity.BadgeId);
    }
}