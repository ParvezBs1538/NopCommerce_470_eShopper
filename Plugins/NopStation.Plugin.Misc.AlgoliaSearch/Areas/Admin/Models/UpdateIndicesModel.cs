using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models
{
    public record UpdateIndicesModel
    {
        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetSearchableAttributeSettings")]
        public bool ResetSearchableAttributeSettings { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetFacetedAttributeSettings")]
        public bool ResetFacetedAttributeSettings { get; set; }
    }
}
