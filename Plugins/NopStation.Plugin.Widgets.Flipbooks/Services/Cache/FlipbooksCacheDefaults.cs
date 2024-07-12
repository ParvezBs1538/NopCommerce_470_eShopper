using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.Flipbooks.Services.Cache
{
    public class FlipbooksCacheDefaults
    {
        public static CacheKey ProductsByFlipbookContentIdKey => new CacheKey("Nopstation.flipbooks.flipbookcontentproducts.products.byflipbookcontentid.{0}-{1}-{2}", FlipbookContentProductPrefix, FlipbookContentPrefix, FlipbookPrefix);
        public static CacheKey FlipbookContentProductsByFlipbookContentIdKey => new CacheKey("Nopstation.flipbooks.flipbookcontentproducts.byflipbookcontentid.{0}", FlipbookContentProductPrefix, FlipbookContentPrefix, FlipbookPrefix);
        public static string FlipbookContentProductPrefix => "Nopstation.flipbooks.flipbookcontentproducts.";

        public static CacheKey FlipbookContentsByFlipbookIdKey => new CacheKey("Nopstation.flipbooks.flipbookcontents.byflipbookid.{0}", FlipbookContentPrefix, FlipbookPrefix);
        public static string FlipbookContentPrefix => "Nopstation.flipbooks.flipbookcontents.";

        public static CacheKey FlipbooksAllKey => new CacheKey("Nopstation.flipbooks.flipbooks.all.{0}-{1}-{2}-{3}-{4}-{5}-{6}", FlipbookPrefix);
        public static string FlipbookPrefix => "Nopstation.flipbooks.flipbooks.";
    }
}