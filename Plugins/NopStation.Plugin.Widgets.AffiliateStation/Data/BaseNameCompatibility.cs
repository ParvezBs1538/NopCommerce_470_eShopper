using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;

namespace NopStation.Plugin.Widgets.AffiliateStation.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(AffiliateCustomer), "NS_Affiliate_AffiliateCustomer" },
            { typeof(CatalogCommission), "NS_Affiliate_CatalogCommission" },
            { typeof(OrderCommission), "NS_Affiliate_OrderCommission" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
