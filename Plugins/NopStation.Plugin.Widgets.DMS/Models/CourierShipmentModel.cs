using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public record CourierShipmentModel : BaseNopEntityModel
    {
        public int ShipperId { get; set; }

        public int ShipmentId { get; set; }

        public string ShipperName { get; set; }

        public int SignaturePictureId { get; set; }

        public string SignaturePictureUrl { get; set; }

        public bool Delivered { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
