using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Services.Cache;

public partial class SmartSliderCacheEventConsumer : CacheEventConsumer<SmartSlider>
{
    protected override async Task ClearCacheAsync(SmartSlider entity)
    {
        await base.RemoveByPrefixAsync(NopStationEntityCacheDefaults<SmartSlider>.Prefix);
        //await RemoveByPrefixAsync(CacheDefaults.SliderAllPrefix);
    }
}