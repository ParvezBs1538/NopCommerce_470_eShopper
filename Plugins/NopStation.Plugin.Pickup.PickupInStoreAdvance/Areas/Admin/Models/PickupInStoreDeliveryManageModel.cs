using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models
{
    public record PickupInStoreDeliveryManageModel : BaseNopEntityModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string ReadyForPickupMarkedAtUtc { get; set; }
        public string PickupUpAtUtc { get; set; }
        public string OrderDate { get; set; }
        public string PickUpStatus { get; set; }
        public string NopOrderStatus { get; set; }
        public int PickUpStatusTypeId { get; set; }
    }
}