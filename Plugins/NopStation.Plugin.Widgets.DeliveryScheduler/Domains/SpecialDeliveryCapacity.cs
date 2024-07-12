using System;
using Nop.Core;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Domains
{
    public class SpecialDeliveryCapacity : BaseEntity, IStoreMappingSupported
    {
        public DateTime SpecialDate { get; set; }

        public int DeliverySlotId { get; set; }

        public int ShippingMethodId { get; set; }

        public int Capacity { get; set; }

        public string Note { get; set; }

        public bool LimitedToStores { get; set; }
    }
}
