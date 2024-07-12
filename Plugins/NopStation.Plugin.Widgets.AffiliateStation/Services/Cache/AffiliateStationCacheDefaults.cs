using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services.Cache
{
    public class AffiliateStationCacheDefaults
    {
        public static CacheKey AffiliateCustomersAllKey => new CacheKey("Nopstation.affiliatestation.affiliatecustomers.all.{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}", AffiliateCustomerPrefix);
        public static CacheKey AffiliateCustomerByCustomerIdKey => new CacheKey("Nopstation.affiliatestation.affiliatecustomers.bycustomerid.{0}", AffiliateCustomerPrefix);
        public static CacheKey AffiliateCustomerByAffiliateIdKey => new CacheKey("Nopstation.affiliatestation.affiliatecustomers.byaffiliateid.{0}", AffiliateCustomerPrefix);
        public static string AffiliateCustomerPrefix => "Nopstation.affiliatestation.affiliatecustomers.";

        public static CacheKey CatalogCommissionAllKey => new CacheKey("Nopstation.affiliatestation.catalogcommissions.all.{0}-{1}-{2}-{3}", CatalogCommissionPrefix);
        public static CacheKey CatalogCommissionByBaseEntityKey => new CacheKey("Nopstation.affiliatestation.catalogcommissions.bybaseentity.{0}-{1}", CatalogCommissionPrefix);
        public static string CatalogCommissionPrefix => "Nopstation.affiliatestation.catalogcommissions.";

        public static CacheKey OrderCommissionAllKey => new CacheKey("Nopstation.affiliatestation.ordercommissions.all.{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}-{11}", OrderCommissionPrefix);
        public static CacheKey OrderCommissionByIdKey => new CacheKey("Nopstation.affiliatestation.ordercommissions.byid.{0}", OrderCommissionPrefix);
        public static string OrderCommissionPrefix => "Nopstation.affiliatestation.ordercommissions.";
    }
}