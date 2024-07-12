using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.SmartSliders.Services.Cache;

public class CacheDefaults
{
    public static CacheKey SliderItemsBySliderIdKey => new CacheKey("Nopstation.smartslider.slideritemss.{0}-{1}-{2}", SliderItemsBySliderIdPrefix);
    public static string SliderItemsBySliderIdPrefix => "Nopstation.smartslider.slideritemss.{0}";
}