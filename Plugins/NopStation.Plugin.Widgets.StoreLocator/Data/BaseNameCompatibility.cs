using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using NopStation.Plugin.Widgets.StoreLocator.Domain;

namespace NopStation.Plugin.Widgets.StoreLocator.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string> 
        {
            { typeof(StoreLocation), "NS_StoreLocation" },
            { typeof(StoreLocationPicture), "NS_StoreLocationPicture" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string> { };
    }
}
