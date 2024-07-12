using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Services.Cache;

public partial class SmartSliderItemCacheEventConsumer : CacheEventConsumer<SmartSliderItem>
{
    protected override async Task ClearCacheAsync(SmartSliderItem entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.SliderItemsBySliderIdPrefix, entity.SliderId);
    }
}