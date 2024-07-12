using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.ProductRequests.Domains;

namespace NopStation.Plugin.Widgets.ProductRequests.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(ProductRequest), "NS_ProductRequest" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
