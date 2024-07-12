using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(ApiCategoryIcon), "NS_WebApi_CategoryIcon" },
            { typeof(ApiDevice), "NS_WebApi_Device" },
            { typeof(ApiSlider), "NS_WebApi_Slider" },
            { typeof(ApiStringResource), "NS_WebApi_StringResource" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
