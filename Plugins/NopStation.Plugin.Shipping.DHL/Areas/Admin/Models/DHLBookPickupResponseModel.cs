using System;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Models
{
    public class DHLBookPickupResponseModel
    {
        public string MessageReference { get; set; }

        public string ConfirmationNumber { get; set; }

        public DateTime? ReadyByTime { get; set; }

        public DateTime? NextPickupDate { get; set; }

        public string Error { get; set; }
    }
}
