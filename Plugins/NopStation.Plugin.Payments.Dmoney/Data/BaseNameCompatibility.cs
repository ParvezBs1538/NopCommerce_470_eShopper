using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Payments.Dmoney.Domains;

namespace NopStation.Plugin.Payments.Dmoney.Data
{
    public partial class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(DmoneyTransaction), "NS_Payment_DmoneyTransaction" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}