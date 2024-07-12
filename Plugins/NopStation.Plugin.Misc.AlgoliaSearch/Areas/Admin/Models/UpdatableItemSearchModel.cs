using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models
{
    public record UpdatableItemSearchModel : BaseSearchModel
    {
        public string EntityName { get; set; }
    }
}
