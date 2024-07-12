namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain.Enum
{
    /// <summary>
    /// Represents the Pickup in store status
    /// </summary>
    public enum PickUpStatusType
    {
        /// <summary>
        /// Order Initied
        /// </summary>
        OrderInitied = 1,

        /// <summary>
        /// Ready For Pick
        /// </summary>
        ReadyForPick = 2,

        /// <summary>
        /// Picked Up By Customer
        /// </summary>
        PickedUpByCustomer = 3,

        /// <summary>
        /// Order Canceled
        /// </summary>
        OrderCanceled = 4
    }
}
