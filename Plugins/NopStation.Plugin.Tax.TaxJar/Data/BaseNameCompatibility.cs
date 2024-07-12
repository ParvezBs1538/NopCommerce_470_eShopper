using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Tax.TaxJar.Domains;

namespace NopStation.Plugin.Tax.TaxJar.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(TaxJarCategory), "NS_TaxJar_Category" },
            { typeof(TaxjarTransactionLog), "NS_TaxJar_TransactionLog" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string> { };
    }
}
