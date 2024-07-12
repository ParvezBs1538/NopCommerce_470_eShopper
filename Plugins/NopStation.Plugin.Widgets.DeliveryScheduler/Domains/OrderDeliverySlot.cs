using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Domains
{
    public class OrderDeliverySlot : BaseEntity
    {
        public int OrderId { get; set; }

        public int DeliverySlotId { get; set; }

        public int ShippingMethodId { get; set; }

        public DateTime DeliveryDate { get; set; }
    }
}
