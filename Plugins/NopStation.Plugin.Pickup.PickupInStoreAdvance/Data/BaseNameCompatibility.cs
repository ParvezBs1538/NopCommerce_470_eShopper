using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(StorePickupPoint), "NS_StorePickupPoint" },
            { typeof(PickupInStoreDeliveryManage), "NS_PickupInStoreDeliveryManage" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
