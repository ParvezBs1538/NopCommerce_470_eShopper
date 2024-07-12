using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(RedxArea), "NS_Redx_Area" },
            { typeof(RedxShipment), "NS_Redx_Shipment" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
