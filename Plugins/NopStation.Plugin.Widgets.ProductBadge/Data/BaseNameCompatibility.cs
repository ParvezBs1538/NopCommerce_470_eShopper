using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
    {
        { typeof(Badge), "NS_Badge" },
        { typeof(BadgeCategoryMapping), "NS_Badge_CategoryMapping" },
        { typeof(BadgeProductMapping), "NS_Badge_ProductMapping" },
        { typeof(BadgeManufacturerMapping), "NS_Badge_ManufacturerMapping" },
        { typeof(BadgeVendorMapping), "NS_Badge_VendorMapping" }
    };

    public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
    {
    };
}