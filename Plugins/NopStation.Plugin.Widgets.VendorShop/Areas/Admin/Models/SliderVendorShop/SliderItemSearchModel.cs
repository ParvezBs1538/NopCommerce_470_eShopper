using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.SliderVendorShop
{
    public record SliderItemSearchModel : BaseSearchModel
    {
        public int SliderId { get; set; }
    }
}
