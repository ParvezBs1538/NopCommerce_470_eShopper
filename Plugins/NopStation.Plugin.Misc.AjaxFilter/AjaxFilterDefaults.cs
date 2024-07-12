using Nop.Core.Caching;

namespace NopStation.Plugin.Misc.AjaxFilter
{
    public static class AjaxFilterDefaults
    {
        public static string CatalogFilters => "catalog_filters_widget_zone";

        public static string FilterViewComponentName => "";

        public static CacheKey ManufacturerProductsNumberCacheKey => new CacheKey("Nop.productmanufacturer.products.number.{0}-{1}-{2}-{3}", ManufacturerProductsNumberPrefix);

        public static string ManufacturerProductsNumberPrefix => "Nop.productmanufacturer.products.number.";

        public static CacheKey SpecificationAttributeProductsNumberCacheKey => new CacheKey("Nop.specificationattribute.products.number.{0}-{1}-{2}-{3}", SpecificationAttributeProductsNumberPrefix);

        public static string SpecificationAttributeProductsNumberPrefix => "Nop.specificationattribute.products.number.";

        public static CacheKey AjaxFilterAvailableSpecificationAttributesCacheKey => new CacheKey("Nop.specificationattribute.products.number.{0}-{1}", AjaxFilterAvailableSpecificationAttributesPrefix);

        public static string AjaxFilterAvailableSpecificationAttributesPrefix => "Nop.specificationattribute.products.number.";

        public static CacheKey AllAjaxFilterSpecificationAttributeIdsFromCategoryId => new CacheKey("Nop.specificationattribute.attributes.{0}", AllAjaxFilterSpecificationAttributeIdsFromCategoryIdPrefix);

        public static string AllAjaxFilterSpecificationAttributeIdsFromCategoryIdPrefix => "Nop.specificationattribute.attributes.";

        public static CacheKey AjaxFilterChildCategoryIdsCacheKey => new CacheKey("Nop.pres.category.childidentifiers-{0}-{1}-{2}", AjaxFilterChildCategoryIdsPrefix);

        public static string AjaxFilterChildCategoryIdsPrefix => "Nop.pres.category.childidentifiers";

        public static CacheKey AjaxFilterOverrideFilterSetCacheKey => new CacheKey("Nop.ajaxfilteroverridefilterset.{0}-{1}-{2}", AjaxFilterOverrideFilterSetPrefix);

        public static CacheKey CategoryModelKey => new CacheKey("Nop.pres.category-{0}-{1}-{2}-{3}", CategoryPrefixCacheKey);

        public static string CategoryPrefixCacheKey => "Nop.pres.category";

        public static string AjaxFilterOverrideFilterSetPrefix => "Nop.ajaxfilteroverridefilterset.";

        public static string PluginSystemName => "NopStation.Plugin.Misc.AjaxFilter";
    }
}
