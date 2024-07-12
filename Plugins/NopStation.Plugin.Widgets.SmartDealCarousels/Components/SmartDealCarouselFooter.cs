using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Components;

public class SmartDealCarouselFooterViewComponent : NopStationViewComponent
{
    private readonly SmartDealCarouselSettings _carouselSettings;

    public SmartDealCarouselFooterViewComponent(SmartDealCarouselSettings carouselSettings)
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
