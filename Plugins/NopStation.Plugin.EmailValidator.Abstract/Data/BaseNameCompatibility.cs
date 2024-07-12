using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.EmailValidator.Abstract.Domains;

namespace NopStation.Plugin.EmailValidator.Abstract.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(AbstractEmail), "NS_EV_Abstract_Email" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
