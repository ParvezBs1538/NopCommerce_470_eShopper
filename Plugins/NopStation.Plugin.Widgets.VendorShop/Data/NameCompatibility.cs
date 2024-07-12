using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.VendorShop.Domains;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Data
{
    public partial class NameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(OCarousel), "NS_VendorShop_OCarousel" },
            { typeof(OCarouselItem), "NS_VendorShop_OCarouselItem" },
            { typeof(Slider), "NS_VendorShop_Anywhere_Slider" },
            { typeof(SliderItem), "NS_VendorShop_Anywhere_SliderItem" },
            { typeof(ProductTab), "NS_VendorShop_ProductTab" },
            { typeof(ProductTabItem), "NS_VendorShop_ProductTabItem" },
            { typeof(ProductTabItemProduct), "NS_VendorShop_ProductTabItemProduct" },
            { typeof(VendorProfile), "NS_VendorShop_VendorProfile" },
            { typeof(VendorSubscriber), "NS_VendorShop_VendorSubscriber" },
            { typeof(VendorFeatureMapping), "NS_VendorShop_VendorFeatureMapping" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}
