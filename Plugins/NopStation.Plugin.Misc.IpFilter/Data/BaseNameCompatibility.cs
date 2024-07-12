using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.IpFilter.Domain;

namespace NopStation.Plugin.Misc.IpFilter.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(IpBlockRule), "NS_IpFilter_IpBlockRule" },
            { typeof(IpRangeBlockRule), "NS_IpFilter_IpRangeBlockRule" },
            { typeof(CountryBlockRule), "NS_IpFilter_CountryBlockRule" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {

        };
    }
}
