using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models
{
    public record MarkedReadyOrPickedModel : BaseNopModel
    {
        public int OrderId { get; set; }
        public int StatusId { get; set; }
    }
}
