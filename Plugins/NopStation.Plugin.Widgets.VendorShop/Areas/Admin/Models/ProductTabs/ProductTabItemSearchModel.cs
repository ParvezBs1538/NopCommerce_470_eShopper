using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.ProductTabs
{
    public record ProductTabItemSearchModel : BaseSearchModel
    {
        public int ProductTabId { get; set; }
    }
}
