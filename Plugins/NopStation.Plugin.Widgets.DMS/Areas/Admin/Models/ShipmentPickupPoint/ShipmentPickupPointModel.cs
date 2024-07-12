using System.ComponentModel.DataAnnotations;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Models.ShipmentPickupPoint
{
    public record ShipmentPickupPointModel : BaseNopEntityModel
    {
        public ShipmentPickupPointModel()
        {
            Address = new AddressModel();
        }

        public AddressModel Address { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.OpeningHours")]
        public string OpeningHours { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(DataFormatString = "{0:F8}", ApplyFormatInEditMode = true)]
        [NopResourceDisplayName("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Latitude")]
        public decimal? Latitude { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(DataFormatString = "{0:F8}", ApplyFormatInEditMode = true)]
        [NopResourceDisplayName("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Longitude")]
        public decimal? Longitude { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Active")]
        public bool Active { get; set; }

        public bool Deleted { get; set; }
    }

}
