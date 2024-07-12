using Nop.Core.Caching;

namespace NopStation.Plugin.Misc.AjaxFilter.Infrastructure.Cache
{
    public class CategoryImagesModelCache
    {
        public static CacheKey CategoryProductImagesCacheKey => new CacheKey("Nop.Plugin.Widgets.FacebookPixel.Configuration-{0}", CategoryProductImagesPrefix);
        public static string CategoryProductImagesPrefix => "Nop.category.product.images";
    }
}
