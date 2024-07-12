using Nop.Core;

namespace NopStation.Plugin.Shipping.Redx.Domains
{
    public class RedxShipment : BaseEntity
    {
        public int OrderId { get; set; }

        public int ShipmentId { get; set; }

        public string TrackingId { get; set; }

        public int RedxAreaId { get; set; }
    }
}