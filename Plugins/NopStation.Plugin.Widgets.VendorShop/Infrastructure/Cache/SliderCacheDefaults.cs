using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.VendorShop.Infrastructure.Cache
{
    public class SliderCacheDefaults
    {
        public static string SliderModelPrefix => "Nopstation.vendorshop.anywhereslider.slidermodel.{0}";
        public static CacheKey SliderBackgrounPictureKey => new("Nopstation.vendorshop.anywhereslider.slidermodel.backgrounpicture.{0}-{1}-{2}", SliderModelPrefix);

        public static CacheKey GetSliderBackgrounPictureKey(int vendorId)
        {
            return new CacheKey("Nopstation.vendorshop.anywhereslider.slidermodel.backgrounpicture.{0}-{1}-{2}",
                string.Format(SliderModelPrefix, vendorId)
                );
        }
        public static CacheKey SliderModelKey => new CacheKey("Nopstation.vendorshop.anywhereslider.slidermodel.byid.{0}-{1}-{2}-{3}-{4}-{5}", SliderModelPrefix);

        public static CacheKey GetSliderModelKey(int vendorId)
        {
            return new CacheKey("Nopstation.vendorshop.anywhereslider.slidermodel.byid.{0}-{1}-{2}-{3}-{4}-{5}",
                string.Format(SliderModelPrefix, vendorId)
                );
        }


    }
}