using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Services.Cache;

public partial class SmartDealCarouselProductCacheEventConsumer : CacheEventConsumer<SmartDealCarouselProductMapping>
{
    protected override async Task ClearCacheAsync(SmartDealCarouselProductMapping entity)
    {
        await RemoveAsync(CacheDefaults.CarouselProductMappingsKey, entity.CarouselId);
    }
}
