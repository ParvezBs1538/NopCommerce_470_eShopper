using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.Core.Domains;

namespace NopStation.Plugin.Misc.Core.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
    {
        { typeof(License), "NS_License" },
    };

    public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
    {
    };
}