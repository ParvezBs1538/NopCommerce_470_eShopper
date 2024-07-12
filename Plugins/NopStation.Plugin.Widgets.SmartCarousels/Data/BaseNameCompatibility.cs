using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
    {
        { typeof(SmartCarousel), "NS_SL_SmartCarousel" },
        { typeof(SmartCarouselProductMapping), "NS_SL_SmartCarouselProduct" },
        { typeof(SmartCarouselManufacturerMapping), "NS_SL_SmartCarouselManufacturer" },
        { typeof(SmartCarouselCategoryMapping), "NS_SL_SmartCarouselCategory" },
        { typeof(SmartCarouselVendorMapping), "NS_SL_SmartCarouselVendor" },
        { typeof(SmartCarouselPictureMapping), "NS_SL_SmartCarouselPicture" }
    };

    public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
    {
    };
}
