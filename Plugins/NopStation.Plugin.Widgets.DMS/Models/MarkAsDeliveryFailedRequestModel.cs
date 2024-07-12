using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public partial record MarkAsDeliveryFailedRequestModel : BaseNopModel
    {
        public int ShipmentId { get; set; }

        //public DateTime TriedToShipOnUtc { get; set; }
        public int DeliverFailedReasonTypeId { get; set; }

        public string Note { get; set; }
    }
}
