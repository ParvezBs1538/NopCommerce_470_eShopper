using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Payout.PayPal.Domain;

namespace NopStation.Plugin.Payout.PayPal.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            {typeof(VendorPayPalConfiguration), "NS_VendorPayPalConfiguration" }
        };
        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string> { };
    }
}
