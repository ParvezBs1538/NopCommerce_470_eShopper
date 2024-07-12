using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Shipping.VendorAndState.Domain;

namespace NopStation.Plugin.Shipping.VendorAndState.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(VendorShipping), "NS_Shipping_VendorShipping" },
            { typeof(VendorStateProvinceShipping), "NS_Shipping_VendorStateProvinceShipping" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
