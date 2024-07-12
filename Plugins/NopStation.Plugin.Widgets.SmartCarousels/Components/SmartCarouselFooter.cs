using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.SmartCarousels.Components;

public class SmartCarouselFooterViewComponent : NopStationViewComponent
{
    private readonly SmartCarouselSettings _carouselSettings;

    public SmartCarouselFooterViewComponent(SmartCarouselSettings carouselSettings)
    {
        _carouselSettings = carouselSettings;
    }

    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (!_carouselSettings.EnableCarousel)
            return Content("");

        return View();
    }
}
