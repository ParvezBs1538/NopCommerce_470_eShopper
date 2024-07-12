using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.SmartSliders.Infrastructure.Cache;

public class ModelCacheDefaults
{
    public static CacheKey SliderOverviewModelKey => new CacheKey("Nopstation.smartslider.model.{0}-overview-{1}-{2}-{3}-{4}", SliderModelPrefix);
    public static CacheKey SliderModelKey => new CacheKey("Nopstation.smartslider.model.{0}-{1}-{2}-{3}-{4}-{5}", SliderModelPrefix);
    public static string SliderModelPrefix => "Nopstation.smartslider.model.{0}";
}
