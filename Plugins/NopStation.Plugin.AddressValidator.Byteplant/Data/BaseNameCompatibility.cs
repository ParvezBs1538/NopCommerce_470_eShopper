using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.AddressValidator.Byteplant.Domains;

namespace NopStation.Plugin.AddressValidator.Byteplant.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(ByteplantAddressExtension), "NS_Validator_ByteplantAddressExtension" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
