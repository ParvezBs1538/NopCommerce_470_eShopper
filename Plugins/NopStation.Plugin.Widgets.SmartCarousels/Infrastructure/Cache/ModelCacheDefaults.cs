using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.SmartCarousels.Infrastructure.Cache;

public class ModelCacheDefaults
{
    public static CacheKey CategoryListModelKey => new CacheKey("Nopstation.smartcarousel.category.list.model.{0}-{1}-{2}-{3}", CategoryListModelPrefix, CarouselCategoryListModelPrefix);
    public static string CategoryListModelPrefix => "Nopstation.smartcarousel.category.list.model.";
    public static string CarouselCategoryListModelPrefix => "Nopstation.smartcarousel.category.list.model.{0}";

    public static CacheKey ManufacturerListModelKey => new CacheKey("Nopstation.smartcarousel.manufacturer.list.model.{0}-{1}-{2}-{3}", ManufacturerListModelPrefix, CarouselManufacturerListModelPrefix);
    public static string ManufacturerListModelPrefix => "Nopstation.smartcarousel.manufacturer.list.model.";
    public static string CarouselManufacturerListModelPrefix => "Nopstation.smartcarousel.manufacturer.list.model.{0}";

    public static CacheKey VendorListModelKey => new CacheKey("Nopstation.smartcarousel.vendor.list.model.{0}-{1}-{2}-{3}", VendorListModelPrefix, CarouselVendorListModelPrefix);
    public static string VendorListModelPrefix => "Nopstation.smartcarousel.vendor.list.model.";
    public static string CarouselVendorListModelPrefix => "Nopstation.smartcarousel.vendor.list.model.{0}";

    public static CacheKey PictureListModelKey => new CacheKey("Nopstation.smartcarousel.picture.list.model.{0}-{1}-{2}-{3}", PictureListModelPrefix, CarouselPictureListModelPrefix);
    public static string PictureListModelPrefix => "Nopstation.smartcarousel.picture.list.model.";
    public static string CarouselPictureListModelPrefix => "Nopstation.smartcarousel.picture.list.model.{0}";

    public static CacheKey CarouselOverviewModelKey => new CacheKey("Nopstation.smartcarousel.model.{0}-overview-{1}-{2}-{3}", CarouselModelPrefix);
    public static CacheKey CarouselModelKey => new CacheKey("Nopstation.smartcarousel.model.{0}-{1}-{2}-{3}", CarouselModelPrefix);
    public static string CarouselModelPrefix => "Nopstation.smartcarousel.model.{0}";
    public static string CarouselByWidgetZoneKey => "Nopstation.smartcarousel.all.Carousel.{0}.{1}.{2}";
    public static string CarouselByWidgetZoneKeyPrefix => "Nopstation.smartcarousel.all.Carousel.";
}
