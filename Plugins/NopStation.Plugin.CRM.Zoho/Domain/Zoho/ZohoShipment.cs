using System;

namespace NopStation.Plugin.CRM.Zoho.Domain.Zoho
{
    public class ZohoShipment : BaseZohoEntity
    {
        public int OrderId { get; set; }

        public string TrackingNumber { get; set; }

        public decimal? TotalWeight { get; set; }

        public DateTime? ShippedDateUtc { get; set; }

        public DateTime? DeliveryDateUtc { get; set; }
    }

    public class ZohoShipmentItem : BaseZohoEntity
    {
        public int ShipmentId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
