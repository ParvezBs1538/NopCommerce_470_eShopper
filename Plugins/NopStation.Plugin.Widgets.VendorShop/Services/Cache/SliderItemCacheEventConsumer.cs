using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Services.Cache
{
    public partial class SliderItemCacheEventConsumer : CacheEventConsumer<SliderItem>
    {
        private readonly ISliderService _sliderService;

        public SliderItemCacheEventConsumer(ISliderService sliderService)
        {
            _sliderService = sliderService;
        }
        protected override async Task ClearCacheAsync(SliderItem entity)
        {
            var slider = await _sliderService.GetSliderByIdAsync(entity.SliderId);

            await RemoveByPrefixAsync(AnywhereSliderCacheDefaults.SliderItemPrefix, slider.VendorId);
            await RemoveByPrefixAsync(AnywhereSliderCacheDefaults.SliderPrefix, slider.VendorId);
        }
    }
}