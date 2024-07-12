using Nop.Services.Shipping.Tracking;
using System.Collections.Generic;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Models
{
    public class DHLTrackingResponseModel
    {
        public DHLTrackingResponseModel()
        {
            ShipmentStatusEvents = new List<ShipmentStatusEvent>();
        }

        public string Error { get; set; }

        public IList<ShipmentStatusEvent> ShipmentStatusEvents { get; set; }
    }
}
