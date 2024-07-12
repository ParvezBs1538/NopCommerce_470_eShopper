using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.CategoryBanners.Domains;

namespace NopStation.Plugin.Widgets.CategoryBanners.Mapping
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(CategoryBanner), "NS_Banner_CategoryBanner" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {

        };
    }
}