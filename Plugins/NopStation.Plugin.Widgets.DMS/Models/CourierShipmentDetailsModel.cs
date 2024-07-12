using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Common;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public record CourierShipmentDetailsModel : BaseNopEntityModel
    {
        public CourierShipmentDetailsModel()
        {
            //Signature = new PictureModel();
            PickupPoint = new ShipmentPickupPointModel();
            ShippingAddress = new AddressModel();
        }

        public int ShipmentId { get; set; }

        public int CourierShipmentStatusId { get; set; }

        public string CourierShipmentStatus { get; set; }

        //public PictureModel Signature { get; set; }

        public int OrderId { get; set; }

        public string TrackingNumber { get; set; }

        public decimal? TotalWeight { get; set; }

        [NopResourceDisplayName("NopStation.DMS.Shipments.ShippedDate")]
        public DateTime? ShippedDate { get; set; }

        [NopResourceDisplayName("NopStation.DMS.Shipments.DeliveryDate")]
        public DateTime? DeliveryDate { get; set; }

        public string CreatedOn { get; set; }

        public string UpdatedOn { get; set; }

        public string ShippingAddressGoogleMapsUrl { get; set; }

        public AddressModel ShippingAddress { get; set; }

        public ShipmentPickupPointModel PickupPoint { get; set; }

        public string PickupPointInfo { get; set; }

        [NopResourceDisplayName("NopStation.DMS.Shipments.CanShip")]
        public bool CanShip { get; set; }

        [NopResourceDisplayName("NopStation.DMS.Shipments.CanDeliver")]
        public bool CanDeliver { get; set; }

        public string PickupTime { get; set; }
    }
}
