using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Models
{
    public record CourierShipmentModel : BaseNopEntityModel
    {
        public CourierShipmentModel()
        {
            AvailableShipmentStatusTypes = new List<SelectListItem>();
            AvailableShippers = new List<SelectListItem>();
            AvailablePickupPoints = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.AddCourierShipment")]
        public bool AddCourierShipment { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.Shipper")]
        public int ShipperId { get; set; }

        public int ShipperNopCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.Shipper")]
        public string ShipperName { get; set; }

        public int ShipmentId { get; set; }


        public string ProofOfDeliveryType { get; set; }
        public bool PODContainPhoto { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.PODPhotoUrl")]
        public string PODPhotoUrl { get; set; }

        //[UIHint("Picture")]
        //[NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.Signature")]
        //public int ProofOfDeliveryReferenceId { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.Signature")]
        //public string SignaturePictureUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.Delivered")]
        public bool Delivered { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentStatus")]
        public int ShipmentStatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentStatus")]
        public string ShipmentStatus { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentPickupPoint")]
        public int ShipmentPickupPointId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.CourierShipments.Fields.TrackingNumber")]
        public string TrackingNumber { get; set; }

        public IList<SelectListItem> AvailableShipmentStatusTypes { get; set; }

        public IList<SelectListItem> AvailableShippers { get; set; }

        public IList<SelectListItem> AvailablePickupPoints { get; set; }
    }
}
