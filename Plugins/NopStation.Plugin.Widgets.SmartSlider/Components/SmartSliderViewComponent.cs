using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.SmartSliders.Factories;

namespace NopStation.Plugin.Widgets.SmartSliders.Components;

public class SmartSliderViewComponent : NopStationViewComponent
{
    private readonly SmartSliderSettings _sliderSettings;
    private readonly ISmartSliderModelFactory _sliderModelFactory;

    public SmartSliderViewComponent(SmartSliderSettings sliderSettings,
        ISmartSliderModelFactory sliderModelFactory)
    {
        _sliderSettings = sliderSettings;
        _sliderModelFactory = sliderModelFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_sliderSettings.EnableSlider)
            return Content("");

        if (_sliderSettings.EnableAjaxLoad)
        {
            var model = await _sliderModelFactory.PrepareSliderAjaxListModelAsync(widgetZone);
            return View("Default.Ajax", model);
        }
        else
        {
            var model = await _sliderModelFactory.PrepareSliderListModelAsync(widgetZone);
            return View(model);
        }
    }
}
