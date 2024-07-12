using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Payments.Quickstream.Domains;

namespace NopStation.Plugin.Payments.Quickstream.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(AcceptedCard), "NS_QuickStream_AcceptedCard" }
        };
        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {

        };
    }
}
