using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.SmartSliders;

public class SmartSliderSettings : ISettings
{
    public SmartSliderSettings()
    {
        SupportedVideoExtensions = new List<string>();
    }

    public bool EnableSlider { get; set; }

    public bool EnableAjaxLoad { get; set; }

    public List<string> SupportedVideoExtensions { get; set; }
}