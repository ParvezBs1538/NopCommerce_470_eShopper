using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Models
{
    public record ShipperModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.DMS.Shippers.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Shippers.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Shippers.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Shippers.Fields.Online")]
        public bool Online { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Shippers.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Shippers.Fields.UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Shippers.Fields.LastLocation")]
        public string LastLocation { get; set; }

        public CourierShipmentSearchModel CourierShipmentSearchModel { get; set; }
        public int GeoMapId { get; set; }
        public string GoogleApiKey { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal LocationUpdateIntervalInSeconds { get; set; }
    }
}
