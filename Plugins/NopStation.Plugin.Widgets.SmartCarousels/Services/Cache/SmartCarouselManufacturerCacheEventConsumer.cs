using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services.Cache;

public partial class SmartCarouselManufacturerCacheEventConsumer : CacheEventConsumer<SmartCarouselManufacturerMapping>
{
    protected override async Task ClearCacheAsync(SmartCarouselManufacturerMapping entity)
    {
        await RemoveAsync(CacheDefaults.CarouselManufacturerMappingsKey, entity.CarouselId);
        await RemoveByPrefixAsync(CacheDefaults.CarouselManufacturersPrefix, entity.CarouselId);
    }
}
