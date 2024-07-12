using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Models
{
    public record DeliverySlotDetailsModel : BaseNopModel
    {
        public DeliverySlotDetailsModel()
        {
            DeliveryCapacities = new Dictionary<string, List<SlotCellModel>>();
        }

        public bool ShowRemainingCapacity { get; set; }

        public int ShippingMethodId { get; set; }

        public DateTime SavedDeliveryDate { get; set; }

        public int SavedShippingMethodId { get; set; }

        public int SavedSlotId { get; set; }

        public IDictionary<string, List<SlotCellModel>> DeliveryCapacities { get; set; }
    }
}
