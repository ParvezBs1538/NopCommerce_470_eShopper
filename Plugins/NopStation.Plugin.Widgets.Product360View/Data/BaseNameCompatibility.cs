using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.Product360View.Domain;

namespace NopStation.Plugin.Widgets.Product360View.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(ProductPictureMapping360), "NS_ProductPictureMapping360" },
            { typeof(ProductImageSetting360),"NS_ProductImageSetting360" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
