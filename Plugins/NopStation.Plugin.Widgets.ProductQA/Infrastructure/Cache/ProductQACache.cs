using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Infrastructure.Cache
{
    public static partial class ProductQACache
    {
        #region ProductQACache

        public static string ProductsQAPrefixCacheKey => "Nop.productqa.";

        public static CacheKey ProductsQAByIdCacheKey => new("Nop.productqa.id-{0}", ProductsQAPrefixCacheKey);

        public static CacheKey ProductQAAllCacheKey => new("Nop.productqa.all-{0}-{1}-{2}", ProductsQAPrefixCacheKey);

        public static CacheKey ProductQAAllByProductIdCacheKey => new("Nop.productqa.allbyproductid-{0}-{1}-{2}-{3}", ProductsQAPrefixCacheKey);
        #endregion
    }
}
