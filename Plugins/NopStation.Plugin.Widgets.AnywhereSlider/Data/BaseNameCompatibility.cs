using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(Slider), "NS_Anywhere_Slider" },
            { typeof(SliderItem), "NS_Anywhere_SliderItem" }
        };
        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {

        };

    }
}
