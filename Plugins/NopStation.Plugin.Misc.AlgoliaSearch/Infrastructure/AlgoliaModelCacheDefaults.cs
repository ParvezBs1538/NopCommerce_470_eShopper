namespace NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure
{
    public class AlgoliaModelCacheDefaults
    {
        public static string AutoCompletePictureModelKey => "Algoliasearch.autocomplete.picture-{0}-{1}";
        public static string AutoCompletePicturePrefixCacheKey => "Algoliasearch.autocomplete.picture";
        public static string AutoCompletePicturePicturePrefixCacheKeyById => "Algoliasearch.autocomplete.picture-{0}-";

        public static string ProductSpecsModelKey => "Algoliasearch.product.specs-{0}-{1}";
        public static string ProductSpecsPrefixCacheKey => "Algoliasearch.product.specs";
        public static string ProductSpecsPrefixCacheKeyById => "Algoliasearch.product.specs-{0}-";

        public static string ProductAttrsModelKey => "Algoliasearch.product.attrs-{0}-{1}";
        public static string ProductAttrsPrefixCacheKey => "Algoliasearch.product.attrs";
        public static string ProductAttrsPrefixCacheKeyById => "Algoliasearch.product.attrs-{0}-";
        public static string ProductAttrscombinationPrefixCacheKey => "Algoliasearch.product.attrcombs";
        public static string ProductAttrscombinationModelKey => "Algoliasearch.product.attrcombs-{0}-{1}";
        public static string ProductAttrscombinationPrefixCacheKeyById => "Algoliasearch.product.attrcombs-{0}-";
    }
}
