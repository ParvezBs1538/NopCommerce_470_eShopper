using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.AnywhereSlider.Factories;
using NopStation.Plugin.Widgets.AnywhereSlider.Helpers;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Components
{
    public class AnywhereSliderViewComponent : NopStationViewComponent
    {
        private readonly ISliderModelFactory _sliderModelFactory;
        private readonly SliderSettings _sliderSettings;

        public AnywhereSliderViewComponent(ISliderModelFactory sliderModelFactory,
            SliderSettings sliderSettings)
        {
            _sliderModelFactory = sliderModelFactory;
            _sliderSettings = sliderSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_sliderSettings.EnableSlider)
                return Content("");

            if (!SliderHelper.TryGetWidgetZoneId(widgetZone, out var widgetZoneId))
                return Content("");

            var model = await _sliderModelFactory.PrepareSliderListModelAsync(widgetZoneId);

            return View(model);
        }
    }
}
