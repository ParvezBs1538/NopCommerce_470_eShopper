using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Services.Cache
{
    public partial class SliderItemCacheEventConsumer : CacheEventConsumer<SliderItem>
    {
        protected override async Task ClearCacheAsync(SliderItem entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<SliderItem>.Prefix);
            await RemoveByPrefixAsync(AnywhereSliderCacheDefaults.SliderItemPrefix);
            await RemoveByPrefixAsync(AnywhereSliderCacheDefaults.SliderPrefix);
        }
    }
}