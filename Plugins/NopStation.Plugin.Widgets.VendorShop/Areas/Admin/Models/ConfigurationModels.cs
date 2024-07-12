using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.Configuration.Fields.EnableOCarousel")]
        public bool EnableOCarousel { get; set; }

        public bool EnableOCarousel_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.Configuration.Fields.EnableSlider")]
        public bool EnableSlider { get; set; }
        public bool EnableSlider_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.Configuration.Fields.EnableProductTabs")]
        public bool EnableProductTabs { get; set; }
        public bool EnableProductTabs_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.Configuration.Fields.EnableVendorShopCampaign")]
        public bool EnableVendorShopCampaign { get; set; }
        public bool EnableVendorShopCampaign_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.Configuration.Fields.EnableVendorCustomCss")]
        public bool EnableVendorCustomCss { get; set; }
        public bool EnableVendorCustomCss_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}