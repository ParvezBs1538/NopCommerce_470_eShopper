using System;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Widgets.DMS.Domain
{
    public class ProofOfDeliveryData : BaseEntity, ISoftDeletedEntity
    {
        public int CourierShipmentId { get; set; }

        public int NopShipmentId { get; set; }

        public int VerifiedByShipperId { get; set; }

        public int ProofOfDeliveryTypeId { get; set; }

        public int ProofOfDeliveryReferenceId { get; set; }

        public DateTime VerifiedOnUtc { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public ProofOfDeliveryTypes ProofOfDeliveryType
        {
            get => (ProofOfDeliveryTypes)ProofOfDeliveryTypeId;
            set => ProofOfDeliveryTypeId = (int)value;
        }
    }
}
