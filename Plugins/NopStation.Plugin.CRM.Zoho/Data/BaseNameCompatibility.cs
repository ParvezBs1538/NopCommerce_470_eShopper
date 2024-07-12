using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.CRM.Zoho.Domain;

namespace NopStation.Plugin.CRM.Zoho.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(UpdatableItem), "NS_ZOHO_UpdatableItem" },
            { typeof(DataMapping), "NS_ZOHO_DataMapping" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
