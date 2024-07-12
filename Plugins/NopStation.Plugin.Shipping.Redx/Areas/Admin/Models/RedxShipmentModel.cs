using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Models
{
    public record RedxShipmentModel : BaseNopEntityModel
    {
        public RedxShipmentModel()
        {
            AvailableRedxAreas = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.Fields.Order")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.Fields.Shipment")]
        public int ShipmentId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.Fields.TrackingId")]
        public string TrackingId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.Fields.RedxArea")]
        public int RedxAreaId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.Fields.RedxArea")]
        public string RedxAreaName { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.Fields.LabelUrl")]
        public string LabelUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.Fields.CustomOrderNumber")]
        public string CustomOrderNumber { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.Fields.OrderStatus")]
        public int OrderStatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxShipments.Fields.OrderStatus")]
        public string OrderStatus { get; set; }

        public bool CanSendShipmentToRedx { get; set; }

        public IList<SelectListItem> AvailableRedxAreas { get; set; }
    }
}