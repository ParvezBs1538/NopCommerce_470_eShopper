using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models
{
    public record ProductTabItemProductSearchModel : BaseSearchModel
    {
        public int ProductTabItemId { get; set; }
    }
}
