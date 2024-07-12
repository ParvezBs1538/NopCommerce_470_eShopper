using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.VendorShop.Services.Cache
{
    public class AnywhereSliderCacheDefaults
    {
        public static CacheKey SliderItemsBySliderIdKey => new("Nopstation.vendorshop.anywhereslider.slideritems.bysliderid.{0}-{1}-{2}-{3}", SliderItemPrefix, SliderPrefix);
        public static string SliderItemPrefix => "Nopstation.vendorshop.anywhereslider.slideritems.{0}";

        public static CacheKey GetSliderItemsBySliderIdKey(int vendorId)
        {
            return new CacheKey("Nopstation.vendorshop.anywhereslider.slideritems.bysliderid.{0}-{1}-{2}-{3}",
                string.Format(SliderItemPrefix, vendorId)
                );
        }
        public static CacheKey SlidersAllKey => new CacheKey("Nopstation.vendorshop.anywhereslider.sliders.all.{0}-{1}-{2}-{3}-{4}-{5}", SliderPrefix);
        public static string SliderPrefix => "Nopstation.vendorshop.anywhereslider.sliders.{0}";

        public static CacheKey GetSlidersAllKey(int vendorId)
        {
            return new CacheKey("Nopstation.vendorshop.anywhereslider.sliders.all.{0}-{1}-{2}-{3}-{4}-{5}",
                string.Format(SliderPrefix, vendorId)
                );
        }
    }
}