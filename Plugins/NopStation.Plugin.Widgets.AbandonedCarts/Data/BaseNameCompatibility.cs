using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(AbandonedCart), "NS_AbandonedCart" },
            { typeof(CustomerAbandonmentInfo),"NS_AbandonedCartCustomerInfo" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
