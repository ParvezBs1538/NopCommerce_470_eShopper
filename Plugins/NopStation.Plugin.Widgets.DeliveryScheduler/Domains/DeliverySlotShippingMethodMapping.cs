using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Domains
{
    public class DeliverySlotShippingMethodMapping : BaseEntity
    {
        public int DeliverySlotId { get; set; }

        public int ShippingMethodId { get; set; }
    }
}
