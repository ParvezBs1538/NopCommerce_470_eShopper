using System;
using Nop.Core;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain.Enum;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain
{
    public class PickupInStoreDeliveryManage : BaseEntity
    {
        public int OrderId { get; set; }
        public DateTime? ReadyForPickupMarkedAtUtc { get; set; }
        public int PickUpStatusTypeId { get; set; }
        public DateTime? CustomerPickedUpAtUtc { get; set; }
        public int? CreatedShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the Pickup Status 
        /// </summary>
        public PickUpStatusType PickUpStatusType
        {
            get => (PickUpStatusType)PickUpStatusTypeId;
            set => PickUpStatusTypeId = (int)value;
        }
    }
}
