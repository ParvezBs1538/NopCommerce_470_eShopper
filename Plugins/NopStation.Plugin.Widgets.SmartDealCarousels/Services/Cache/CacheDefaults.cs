using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Services.Cache;

public class CacheDefaults
{
    public static CacheKey CarouselProductMappingsKey => new CacheKey("Nopstation.smartcarousel.products.mappings.byocarouselid.{0}", CarouselProductMappingsPrefix);
    public static string CarouselProductMappingsPrefix => "Nopstation.smartcarousel.products.mappings.";
}
