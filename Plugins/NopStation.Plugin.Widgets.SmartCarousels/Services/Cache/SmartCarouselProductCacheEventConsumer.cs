using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services.Cache;

public partial class SmartCarouselProductCacheEventConsumer : CacheEventConsumer<SmartCarouselProductMapping>
{
    protected override async Task ClearCacheAsync(SmartCarouselProductMapping entity)
    {
        await RemoveAsync(CacheDefaults.CarouselProductMappingsKey, entity.CarouselId);
        await RemoveByPrefixAsync(CacheDefaults.CarouselProductsPrefix, entity.CarouselId);
    }
}
