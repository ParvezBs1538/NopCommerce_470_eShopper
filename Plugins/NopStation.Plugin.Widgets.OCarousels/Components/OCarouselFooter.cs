using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.OCarousels.Components
{
    public class OCarouselFooterViewComponent : NopStationViewComponent
    {
        private readonly OCarouselSettings _carouselSettings;

        public OCarouselFooterViewComponent(OCarouselSettings carouselSettings)
        {
            _carouselSettings = carouselSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_carouselSettings.EnableOCarousel)
                return Content("");

            return View();
        }
    }
}
