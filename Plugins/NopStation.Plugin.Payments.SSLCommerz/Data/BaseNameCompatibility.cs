using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Payments.SSLCommerz.Domains;

namespace NopStation.Plugin.Payments.SSLCommerz.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(Refund), "NS_Payment_SSLCommerz_Refund" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
