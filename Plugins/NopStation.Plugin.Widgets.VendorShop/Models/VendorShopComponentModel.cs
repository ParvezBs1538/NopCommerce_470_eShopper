using System.Collections.Generic;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Widgets.VendorShop.Models.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Models.ProductTabs;
using NopStation.Plugin.Widgets.VendorShop.Models.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Models
{
    public record VendorShopComponentModel : BaseNopModel
    {
        public OCarouselListModel OCarouselListModel { get; set; }
        public SliderListModel SliderListModel { get; set; }
        public IList<ProductTabModel> ProductTabsModels { get; set; }
    }
}
