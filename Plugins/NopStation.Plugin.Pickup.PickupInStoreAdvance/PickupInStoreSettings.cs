using Nop.Core.Configuration;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance
{
    public class PickupInStoreSettings : ISettings
    {
        public bool ShowOrderStatusInOrderDetails { get; set; }
        public bool AddOrderNote { get; set; }
        public bool OrderNotesShowToCustomer { get; set; }
        public bool NotifyCustomerIfReady { get; set; }
    }
}
