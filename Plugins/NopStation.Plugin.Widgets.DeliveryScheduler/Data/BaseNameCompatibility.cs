using System;
using System.Collections.Generic;
using System.Text;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(DeliverySlot), "NS_DSM_DeliverySlot" },
            { typeof(DeliveryCapacity), "NS_DSM_DeliveryCapacity" },
            { typeof(SpecialDeliveryCapacity), "NS_DSM_SpecialDeliveryCapacity" },
            { typeof(SpecialDeliveryOffset), "NS_DSM_SpecialDeliveryOffset" },
            { typeof(OrderDeliverySlot), "NS_DSM_OrderDeliverySlot" },
            { typeof(DeliverySlotShippingMethodMapping), "NS_DSM_DeliverySlot_ShippingMethod_Mapping" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
