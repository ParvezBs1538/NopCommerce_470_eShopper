using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.VendorShop.Services.Cache
{
    public static class VendorShopFeatureCacheDefault
    {
        public static CacheKey GetVendorShopIsEnabledCacheKey(int vendorId)
        {
            return new CacheKey(string.Format(VendorShopIsEnabledKey, vendorId), GetVendorShopIsEnabledCachePrefix(vendorId));
        }
        public static string GetVendorShopIsEnabledCachePrefix(int vendorId)
        {
            return string.Format(VendorShopIsEnabledPrefix, vendorId);
        }
        private static string VendorShopIsEnabledKey => "Nopstation.vendorshop.VendorShopIsEnabledKey.{0}";
        private static string VendorShopIsEnabledPrefix => "Nopstation.vendorshop.VendorShopIsEnabledKey.{0}";
    }
}
