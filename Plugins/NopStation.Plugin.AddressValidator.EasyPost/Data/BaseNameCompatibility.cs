using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.AddressValidator.EasyPost.Domains;

namespace NopStation.Plugin.AddressValidator.EasyPost.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(EasyPostAddressExtension), "NS_Validator_EasyPostAddressExtension" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
