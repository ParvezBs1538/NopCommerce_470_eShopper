using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services.Cache;

public partial class SmartCarouselPictureCacheEventConsumer : CacheEventConsumer<SmartCarouselPictureMapping>
{
    protected override async Task ClearCacheAsync(SmartCarouselPictureMapping entity)
    {
        await RemoveAsync(CacheDefaults.CarouselPictureMappingsKey, entity.CarouselId);
    }
}
