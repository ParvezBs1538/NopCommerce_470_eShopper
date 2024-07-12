using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.ProductTabs
{
    public record ProductTabItemProductSearchModel : BaseSearchModel
    {
        public int ProductTabItemId { get; set; }
    }
}
