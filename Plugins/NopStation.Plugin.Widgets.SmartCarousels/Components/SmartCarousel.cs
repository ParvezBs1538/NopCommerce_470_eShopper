using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.SmartCarousels.Factories;

namespace NopStation.Plugin.Widgets.SmartCarousels.Components;

public class SmartCarouselViewComponent : NopStationViewComponent
{
    private readonly ISmartCarouselModelFactory _carouselModelFactory;
    private readonly SmartCarouselSettings _carouselSettings;

    public SmartCarouselViewComponent(ISmartCarouselModelFactory carouselModelFactory,
        SmartCarouselSettings carouselSettings)
    {
        _carouselModelFactory = carouselModelFactory;
        _carouselSettings = carouselSettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_carouselSettings.EnableCarousel)
            return Content("");

        if (_carouselSettings.EnableAjaxLoad)
        {
            var model = await _carouselModelFactory.PrepareCarouselAjaxListModelAsync(widgetZone);
            return View("Default.Ajax", model);
        }
        else
        {
            var model = await _carouselModelFactory.PrepareCarouselListModelAsync(widgetZone);
            return View(model);
        }
    }
}
