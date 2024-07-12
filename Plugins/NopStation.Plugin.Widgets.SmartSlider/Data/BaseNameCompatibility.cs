using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
    {
        { typeof(SmartSlider), "NS_SL_SmartSlider" },
        { typeof(SmartSliderItem), "NS_SL_SmartSliderItem" },
    };
    public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
    {

    };

}
