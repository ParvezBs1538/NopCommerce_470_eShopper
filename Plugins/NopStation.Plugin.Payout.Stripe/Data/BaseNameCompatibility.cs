using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Payout.Stripe.Domain;

namespace NopStation.Plugin.Payout.Stripe.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            {typeof(VendorStripeConfiguration), "NS_VendorStripeConfiguration" }
        };
        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string> { };
    }
}
