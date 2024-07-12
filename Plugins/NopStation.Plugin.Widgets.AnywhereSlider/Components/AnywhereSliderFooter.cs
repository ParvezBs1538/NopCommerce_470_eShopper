using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Components
{
    public class AnywhereSliderFooterViewComponent : NopStationViewComponent
    {
        private readonly SliderSettings _sliderSettings;

        public AnywhereSliderFooterViewComponent(SliderSettings sliderSettings)
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
}
