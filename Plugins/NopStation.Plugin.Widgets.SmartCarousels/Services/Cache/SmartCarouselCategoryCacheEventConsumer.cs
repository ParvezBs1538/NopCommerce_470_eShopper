using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services.Cache;

public partial class SmartCarouselCategoryCacheEventConsumer : CacheEventConsumer<SmartCarouselCategoryMapping>
{
    protected override async Task ClearCacheAsync(SmartCarouselCategoryMapping entity)
    {
        await RemoveAsync(CacheDefaults.CarouselCategoryMappingsKey, entity.CarouselId);
        await RemoveByPrefixAsync(CacheDefaults.CarouselCategoriesPrefix, entity.CarouselId);
    }
}
