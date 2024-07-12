using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.VendorShop
{
    public class VendorShopSettings : ISettings
    {
        public bool EnableOCarousel { get; set; }
        public bool EnableSlider { get; set; }
        public bool EnableProductTabs { get; set; }
        public bool EnableVendorShopCampaign { get; set; }
        public bool EnableVendorCustomCss { get; set; }
    }
}
