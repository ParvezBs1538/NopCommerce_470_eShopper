using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Services.Cache
{
    public class AnywhereSliderCacheDefaults
    {
        public static CacheKey SliderItemsBySliderIdKey => new CacheKey("Nopstation.anywhereslider.slideritems.bysliderid.{0}-{1}-{2}", SliderItemPrefix, SliderPrefix);
        public static string SliderItemPrefix => "Nopstation.anywhereslider.slideritems.";

        public static CacheKey SlidersAllKey => new CacheKey("Nopstation.anywhereslider.sliders.all.{0}-{1}-{2}-{3}-{4}", SliderPrefix);
        public static string SliderPrefix => "Nopstation.anywhereslider.sliders.";
    }
}