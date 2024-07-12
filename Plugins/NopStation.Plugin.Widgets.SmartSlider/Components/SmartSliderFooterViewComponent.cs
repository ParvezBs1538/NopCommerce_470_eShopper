using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.SmartSliders.Components;

public class SmartSliderFooterViewComponent : NopStationViewComponent
{
    private readonly SmartSliderSettings _sliderSettings;

    public SmartSliderFooterViewComponent(SmartSliderSettings sliderSettings)
    {
        _sliderSettings = sliderSettings;
    }

    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (!_sliderSettings.EnableSlider)
            return Content("");

        return View();
    }
}
