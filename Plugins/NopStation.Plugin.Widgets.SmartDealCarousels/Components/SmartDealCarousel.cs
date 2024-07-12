using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.SmartDealCarousels.Factories;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Components;

public class SmartDealCarouselViewComponent : NopStationViewComponent
{
    private readonly ISmartDealCarouselModelFactory _carouselModelFactory;
    private readonly SmartDealCarouselSettings _carouselSettings;

    public SmartDealCarouselViewComponent(ISmartDealCarouselModelFactory carouselModelFactory,
        SmartDealCarouselSettings carouselSettings)
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
