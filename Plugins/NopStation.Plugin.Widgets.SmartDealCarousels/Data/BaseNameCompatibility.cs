using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
    {
        { typeof(SmartDealCarousel), "NS_SL_SmartDealCarousel" },
        { typeof(SmartDealCarouselProductMapping), "NS_SL_SmartDealCarouselProduct" }
    };

    public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
    {
    };
}
