using Nop.Core.Caching;

namespace NopStation.Plugin.Tax.TaxJar.Services.Cache
{
    public class TaxJarCacheDefaults
    {
        public static CacheKey CategoryFormattedListKey => new CacheKey("Nopstation.Taxjar.category.formatted.all.{0}-{1}", CategoryPrefix);
        public static CacheKey CategoryListKey => new CacheKey("Nopstation.Taxjar.category.all.{0}-{1}", CategoryPrefix);
        public static CacheKey CategoryByValueKey => new CacheKey("Nopstation.Taxjar.category.byvalue.{0}", CategoryPrefix);
        public static CacheKey CategoryByCategoryIdKey => new CacheKey("Nopstation.Taxjar.category.bycategoryid.{0}", CategoryPrefix);
        public static string CategoryPrefix => "Nopstation.Taxjar.category.";
    }
}