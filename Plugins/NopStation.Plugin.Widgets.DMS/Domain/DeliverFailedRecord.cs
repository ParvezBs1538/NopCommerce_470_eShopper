using System;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Widgets.DMS.Domain
{
    public class DeliverFailedRecord : BaseEntity, ISoftDeletedEntity
    {
        public int ShipmentId { get; set; }

        public int ShipperId { get; set; }

        public int CourierShipmentId { get; set; }

        public int DeliverFailedReasonId { get; set; }

        public string Note { get; set; }

        public DateTime DeliveryFailedOnUtc { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public DeliverFailedReasonTypes DeliverFailedReasonType
        {
            get => (DeliverFailedReasonTypes)DeliverFailedReasonId;
            set => DeliverFailedReasonId = (int)value;
        }
    }
}
