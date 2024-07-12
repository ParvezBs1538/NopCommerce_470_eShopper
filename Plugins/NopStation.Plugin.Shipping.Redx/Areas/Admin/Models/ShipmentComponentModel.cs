using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Models
{
    public record ShipmentComponentModel : BaseNopEntityModel
    {
        public int OrderId { get; set; }

        public bool CanSendShipmentToRedx { get; set; }
    }
}
