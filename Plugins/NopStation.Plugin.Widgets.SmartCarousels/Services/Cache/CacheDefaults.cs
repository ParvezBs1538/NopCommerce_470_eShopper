using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services.Cache;

public class CacheDefaults
{
    public static CacheKey CarouselProductsKey => new CacheKey("Nopstation.smartcarousel.products.byocarouselid.{0}-{1}-{2}-{3}-{4}", CarouselProductsPrefix, CarouselProductMappingsPrefix);
    public static string CarouselProductsPrefix => "Nopstation.smartcarousel.products.byocarouselid.{0}";

    public static CacheKey CarouselProductMappingsKey => new CacheKey("Nopstation.smartcarousel.products.mappings.byocarouselid.{0}", CarouselProductMappingsPrefix);
    public static string CarouselProductMappingsPrefix => "Nopstation.smartcarousel.products.mappings.";

    public static CacheKey CarouselManufacturersKey => new CacheKey("Nopstation.smartcarousel.manufacturers.byocarouselid.{0}-{1}-{2}-{3}-{4}", CarouselManufacturersPrefix, CarouselManufacturerMappingsPrefix);
    public static string CarouselManufacturersPrefix => "Nopstation.smartcarousel.manufacturers.byocarouselid.{0}";

    public static CacheKey CarouselManufacturerMappingsKey => new CacheKey("Nopstation.smartcarousel.manufacturers.mappings.byocarouselid.{0}", CarouselManufacturerMappingsPrefix);
    public static string CarouselManufacturerMappingsPrefix => "Nopstation.smartcarousel.manufacturers.mappings.";

    public static CacheKey CarouselCategoriesKey => new CacheKey("Nopstation.smartcarousel.categories.byocarouselid.{0}-{1}-{2}-{3}-{4}", CarouselCategoriesPrefix, CarouselCategoryMappingsPrefix);
    public static string CarouselCategoriesPrefix => "Nopstation.smartcarousel.categories.byocarouselid.{0}";

    public static CacheKey CarouselCategoryMappingsKey => new CacheKey("Nopstation.smartcarousel.categories.mappings.byocarouselid.{0}", CarouselCategoryMappingsPrefix);
    public static string CarouselCategoryMappingsPrefix => "Nopstation.smartcarousel.categories.mappings.";

    public static CacheKey CarouselVendorsKey => new CacheKey("Nopstation.smartcarousel.vendors.byocarouselid.{0}-{1}-{2}", CarouselVendorsPrefix, CarouselVendorMappingsPrefix);
    public static string CarouselVendorsPrefix => "Nopstation.smartcarousel.vendors.byocarouselid.{0}";

    public static CacheKey CarouselVendorMappingsKey => new CacheKey("Nopstation.smartcarousel.vendormappings.byocarouselid.{0}", CarouselVendorMappingsPrefix);
    public static string CarouselVendorMappingsPrefix => "Nopstation.smartcarousel.vendors.mappings.";

    public static CacheKey CarouselPictureMappingsKey => new CacheKey("Nopstation.smartcarousel.picturemappings.byocarouselid.{0}");
}
