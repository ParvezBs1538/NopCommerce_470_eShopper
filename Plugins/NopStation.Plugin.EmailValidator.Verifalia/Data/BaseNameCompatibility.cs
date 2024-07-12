using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.EmailValidator.Verifalia.Domains;

namespace NopStation.Plugin.EmailValidator.Verifalia.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(VerifaliaEmail), "NS_EV_Verifalia_Email" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
