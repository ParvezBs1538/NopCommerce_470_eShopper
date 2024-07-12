using Nop.Core.Caching;

namespace NopStation.Plugin.Shipping.VendorAndState.Services.Cache
{
    public class ShippingByVendorCacheDefaults
    {
        public static CacheKey VendorStateProvinceShippingVendorAndShippingIdKey => new CacheKey("Nopstation.shippingbyvendor.vendorstateprovinceshippings.byvendorandshippingid.{0}-{1}-{2}", VendorStateProvinceShippingPrefixKey, VendorShippingPrefix);
        public static CacheKey VendorStateProvinceShippingAllKey => new CacheKey("Nopstation.shippingbyvendor.vendorstateprovinceshippings.all.{0}-{1}-{2}-{3}-{4}-{5}", VendorStateProvinceShippingPrefixKey, VendorShippingPrefix);
        public static string VendorStateProvinceShippingPrefixKey => "Nopstation.shippingbyvendor.vendorstateprovinceshippings.";

        public static CacheKey VendorShippingByVendorAndShippingIdKey => new CacheKey("Nopstation.shippingbyvendor.vendorshippings.byvendorandshippingid.{0}-{1}", VendorShippingPrefix);
        public static CacheKey VendorShippingsAllKey => new CacheKey("Nopstation.shippingbyvendor.vendorshippings.all.{0}-{1}-{2}-{3}-{4}", VendorShippingPrefix);
        public static string VendorShippingPrefix => "Nopstation.shippingbyvendor.vendorshippings.";
    }
}