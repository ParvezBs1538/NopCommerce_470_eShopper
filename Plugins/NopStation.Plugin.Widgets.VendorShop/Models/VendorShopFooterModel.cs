using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Models
{
    public record VendorShopFooterModel : BaseNopModel
    {
        public bool EnableSlider { get; set; }
        public bool EnableCarousel { get; set; }
        public bool EnableProductTabs { get; set; }
    }
}
