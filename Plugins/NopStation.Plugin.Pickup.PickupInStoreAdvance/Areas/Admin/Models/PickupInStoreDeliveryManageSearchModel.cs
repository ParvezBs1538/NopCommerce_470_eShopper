using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models
{
    public record PickupInStoreDeliveryManageSearchModel : BaseSearchModel
    {
        public PickupInStoreDeliveryManageSearchModel()
        {
            AvailableSearchStatus = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.SearchOrder.OrderId")]
        public string SearchOrderId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.SearchOrder.SearchStatusId")]
        public string SearchStatusId { get; set; }

        public IList<SelectListItem> AvailableSearchStatus { get; set; }
    }
}
