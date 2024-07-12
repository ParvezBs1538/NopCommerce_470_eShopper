using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Models
{
    public record PickupStatusModel : BaseNopModel
    {
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string ReadyTime { get; set; }
        public string DeliveryTime { get; set; }
    }
}
