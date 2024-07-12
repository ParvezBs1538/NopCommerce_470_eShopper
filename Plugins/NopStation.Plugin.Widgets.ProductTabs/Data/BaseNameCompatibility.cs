using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(ProductTab), "NS_ProductTab" },
            { typeof(ProductTabItem), "NS_ProductTabItem" },
            { typeof(ProductTabItemProduct), "NS_ProductTabItemProduct" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
