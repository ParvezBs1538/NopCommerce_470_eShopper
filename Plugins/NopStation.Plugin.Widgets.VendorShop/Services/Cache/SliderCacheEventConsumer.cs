using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Services.Cache
{
    public partial class SliderCacheEventConsumer : CacheEventConsumer<Slider>
    {
        protected override async Task ClearCacheAsync(Slider entity)
        {
            await RemoveByPrefixAsync(AnywhereSliderCacheDefaults.SliderPrefix, entity.VendorId);
        }
    }
}