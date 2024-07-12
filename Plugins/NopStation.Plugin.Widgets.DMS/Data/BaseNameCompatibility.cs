using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(Shipper), "NS_DMS_Shipper" },
            { typeof(DeliverFailedRecord), "NS_DMS_DeliverFailedRecord" },
            { typeof(OTPRecord), "NS_DMS_OTPRecord" },
            { typeof(ProofOfDeliveryData), "NS_DMS_ProofOfDeliveryData" },
            { typeof(ShipmentNote), $"NS_DMS_ShipmentNote" },
            { typeof(ShipmentPickupPoint), $"NS_DMS_ShipmentPickupPoint" },
            { typeof(ShipperDevice), $"NS_DMS_ShipperDevice" },
            { typeof(CourierShipment), "NS_DMS_CourierShipment" },
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {
        };
    }
}
