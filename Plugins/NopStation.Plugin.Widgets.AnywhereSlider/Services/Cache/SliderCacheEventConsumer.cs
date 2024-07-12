using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Services.Cache
{
    public partial class SliderCacheEventConsumer : CacheEventConsumer<Slider>
    {
        protected override async Task ClearCacheAsync(Slider entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<Slider>.Prefix);
            await RemoveByPrefixAsync(AnywhereSliderCacheDefaults.SliderPrefix);
        }
    }
}