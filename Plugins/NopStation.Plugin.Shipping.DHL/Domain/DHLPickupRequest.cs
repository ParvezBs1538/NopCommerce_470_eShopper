using System;
using Nop.Core;

namespace NopStation.Plugin.Shipping.DHL.Domain
{
    public class DHLPickupRequest : BaseEntity
    {
        public int OrderId { get; set; }

        public string MessageReference { get; set; }

        public string ConfirmationNumber { get; set; }

        public DateTime? ReadyByTime { get; set; }

        public DateTime? NextPickupDate { get; set; }
    }
}
