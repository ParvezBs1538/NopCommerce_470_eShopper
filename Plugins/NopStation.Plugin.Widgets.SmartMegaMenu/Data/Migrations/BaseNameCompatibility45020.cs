using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.SmartMegaMenu.Data.Migrations.OldDomain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Data.Migrations;

public class BaseNameCompatibility45020 : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
    {
        { typeof(Menu), "NS_SmartMegaMenu" }
    };

    public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
    {

    };
}
