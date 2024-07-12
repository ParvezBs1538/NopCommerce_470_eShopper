using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Shipping.DHL.Domain;

namespace NopStation.Plugin.Shipping.DHL.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(DHLShipment), "NS_DHL_Shipment" },
            { typeof(DHLService), "NS_DHL_Service" },
            { typeof(DHLPickupRequest), "NS_DHL_PickupRequest" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
