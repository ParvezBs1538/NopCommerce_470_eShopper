using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.ProductRibbon
{
    public class ProductRibbonDefaults
    {
        public static CacheKey BestSellerKey = new CacheKey("NopStation.productribbon.bestseller-{0}-{1}-{2}-{3}-{4}-{5}", BestSellerPrefixCacheKey);
        public static string BestSellerPrefixCacheKey = "NopStation.productribbon.bestseller";
    }
}
