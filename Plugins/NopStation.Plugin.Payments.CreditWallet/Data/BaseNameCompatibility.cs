using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Data
{
    public partial class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(Wallet), "NS_Wallet" },
            { typeof(InvoicePayment), "NS_Wallet_InvoicePayment" },
            { typeof(ActivityHistory), "NS_Wallet_ActivityHistory" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
