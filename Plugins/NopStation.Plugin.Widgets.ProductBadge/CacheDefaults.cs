using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.ProductBadge;

public class CacheDefaults
{
    public static CacheKey BadgeBestSellingProductKey => new CacheKey("Nopstation.productbadge.badges.bestselling-{0}-{1}");

    public static CacheKey ActiveBadgesKey => new CacheKey("Nopstation.productbadge.badges.active-{0}-{1}-{2}", ActiveBadgePrefix);
    public static string ActiveBadgePrefix => "Nopstation.productbadge.badges.active";

    public static CacheKey BadgeProductMappingsKey => new CacheKey("Nopstation.productbadge.products.mappings-{0}", BadgeProductsPrefix, BadgeProductsAllPrefix);
    public static CacheKey BadgeProductsKey => new CacheKey("Nopstation.productbadge.products-{0}", BadgeProductsPrefix, BadgeProductsAllPrefix);
    public static string BadgeProductsPrefix => "Nopstation.productbadge.products-{0}";
    public static string BadgeProductsAllPrefix => "Nopstation.productbadge.products";

    public static CacheKey BadgeCategoryMappingsKey => new CacheKey("Nopstation.productbadge.categories.mappings-{0}-{1}", BadgeCategoriesPrefix, BadgeCategoriesAllPrefix);
    public static CacheKey BadgeCategoriesKey => new CacheKey("Nopstation.productbadge.categories-{0}-{1}", BadgeCategoriesPrefix, BadgeCategoriesAllPrefix);
    public static string BadgeCategoriesPrefix => "Nopstation.productbadge.categories-{0}";
    public static string BadgeCategoriesAllPrefix => "Nopstation.productbadge.categories";

    public static CacheKey BadgeManufacturerMappingsKey => new CacheKey("Nopstation.productbadge.manufacturers.mappings-{0}-{1}", BadgeManufacturersPrefix, BadgeManufacturersAllPrefix);
    public static CacheKey BadgeManufacturersKey => new CacheKey("Nopstation.productbadge.manufacturers-{0}-{1}", BadgeManufacturersPrefix, BadgeManufacturersAllPrefix);
    public static string BadgeManufacturersPrefix => "Nopstation.productbadge.manufacturers-{0}";
    public static string BadgeManufacturersAllPrefix => "Nopstation.productbadge.manufacturers";

    public static CacheKey BadgeVendorMappingsKey => new CacheKey("Nopstation.productbadge.vendors.mappings-{0}-{1}", BadgeVendorsPrefix, BadgeVendorsAllPrefix);
    public static CacheKey BadgeVendorsKey => new CacheKey("Nopstation.productbadge.vendors-{0}-{1}", BadgeVendorsPrefix, BadgeVendorsAllPrefix);
    public static string BadgeVendorsPrefix => "Nopstation.productbadge.vendors-{0}";
    public static string BadgeVendorsAllPrefix => "Nopstation.productbadge.vendors";

    public static CacheKey BadgeModelKey => new CacheKey("Nopstation.productbadge.model-{0}-{1}-{2}-{3}-{4}-{5}", BadgeModelPrefix);
    public static string BadgeModelPrefix => "Nopstation.productbadge.model-{0}";
}