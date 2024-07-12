using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.Popups.Domains;

namespace NopStation.Plugin.Widgets.Popups.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
    {
        { typeof(Popup), "NS_Popup" }
    };

    public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
    {

    };
}
