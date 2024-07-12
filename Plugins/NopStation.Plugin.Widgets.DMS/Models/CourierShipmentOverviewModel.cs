using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public record CourierShipmentOverviewModel : BaseNopEntityModel
    {
        public CourierShipmentOverviewModel()
        {
            PickupPointInfo = "";
            PickupTime = "";
            DeliveryAddress = "";
            ShipToName = "";
        }

        public int ShipmentId { get; set; }

        public string TrackingNumber { get; set; }

        public string CustomOrderNumber { get; set; }

        public string ShippingStatus { get; set; }

        public string PickupPointInfo { get; set; }

        public string PickupTime { get; set; }

        public DateTime? PickedUpOn { get; set; }

        public DateTime? DeliveredOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public int CourierShipmentStatusId { get; set; }

        public string CourierShipmentStatus { get; set; }

        public string ReceiverName { get; set; }

        public string DeliveryAddress { get; set; }

        public int DeliveryAddressId { get; set; }

        public string ShipToName { get; set; }
        public int DeliverFailedReasonId { get; set; }
        public string DeliverFailedReasonString { get; set; }
        public string Note { get; set; }
    }
}
