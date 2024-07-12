using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.AddressValidator.Google.Domains;

namespace NopStation.Plugin.AddressValidator.Google.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(GoogleAddressExtension), "NS_Validator_GoogleAddressExtension" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
