using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Infrastructure.Cache;

public class ModelCacheDefaults
{
    public static CacheKey CarouselOverviewModelKey => new CacheKey("Nopstation.smartdealcarousel.model.{0}-overview-{1}-{2}-{3}-{4}", CarouselModelPrefix);
    public static CacheKey CarouselModelKey => new CacheKey("Nopstation.smartdealcarousel.model.{0}-{1}-{2}-{3}-{4}", CarouselModelPrefix);
    public static string CarouselModelPrefix => "Nopstation.smartdealcarousel.model.{0}";
}
