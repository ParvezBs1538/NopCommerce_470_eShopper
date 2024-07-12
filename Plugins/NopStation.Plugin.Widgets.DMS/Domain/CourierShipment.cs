using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.DMS.Domain
{
    public class CourierShipment : BaseEntity
    {
        public int ShipmentId { get; set; }

        public int ShipperId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public int ShipmentStatusId { get; set; }

        public int ShipmentPickupPointId { get; set; }

        public ShipmentStatusTypes ShipmentStatusType
        {
            get => (ShipmentStatusTypes)ShipmentStatusId;
            set => ShipmentStatusId = (int)value;
        }
    }
}
