using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services.Cache;

public partial class SmartCarouselVendorCacheEventConsumer : CacheEventConsumer<SmartCarouselVendorMapping>
{
    protected override async Task ClearCacheAsync(SmartCarouselVendorMapping entity)
    {
        await RemoveAsync(CacheDefaults.CarouselVendorMappingsKey, entity.CarouselId);
        await RemoveByPrefixAsync(CacheDefaults.CarouselVendorsPrefix, entity.CarouselId);
    }
}
