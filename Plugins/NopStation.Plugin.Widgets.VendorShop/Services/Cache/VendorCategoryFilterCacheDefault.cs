using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.VendorShop.Services.Cache
{
    public static class VendorCategoryFilterCacheDefault
    {
        public static CacheKey GetAllVendorCategoryCacheKey(int vendorId, int storeId = 0)
        {
            return new CacheKey(string.Format(VendorCategoryAllKey, vendorId, storeId), GetAllVendorCategoryCacheKeyPrefix(vendorId));
        }
        public static string GetAllVendorCategoryCacheKeyPrefix(int vendorId)
        {
            return string.Format(VendorCategoryAllPrefix, vendorId);
        }
        private static string VendorCategoryAllKey => "Nopstation.vendorshop.VendorCategoryFilter.All.{0}.{1}";
        private static string VendorCategoryAllPrefix => "Nopstation.vendorshop.VendorCategoryFilter.All.{0}";
    }
}
