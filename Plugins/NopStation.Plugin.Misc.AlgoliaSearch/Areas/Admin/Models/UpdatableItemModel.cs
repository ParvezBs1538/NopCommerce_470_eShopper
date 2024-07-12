using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models
{
    public record UpdatableItemModel : BaseNopEntityModel
    {
        public int EntityId { get; set; }

        public string EntityName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.UpdatableItem.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.UpdatableItem.UpdatedBy")]
        public int LastUpdatedBy { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.UpdatableItem.UpdatedBy")]
        public string UpdatedByCustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.UpdatableItem.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }
    }
}
