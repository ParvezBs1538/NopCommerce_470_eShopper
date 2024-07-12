using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Services.Cache;

public partial class BadgeVendorMappingCacheEventConsumer : CacheEventConsumer<BadgeVendorMapping>
{
    protected override async Task ClearCacheAsync(BadgeVendorMapping entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.BadgeVendorsPrefix, entity.BadgeId);
    }
}