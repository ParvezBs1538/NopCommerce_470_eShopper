using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.OCarousels.Factories;
using NopStation.Plugin.Widgets.OCarousels.Helpers;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels.Components
{
    public class OCarouselViewComponent : NopStationViewComponent
    {
        private readonly IStoreContext _storeContex;
        private readonly IOCarouselService _carouselService;
        private readonly IOCarouselModelFactory _carouselModelFactory;
        private readonly OCarouselSettings _carouselSettings;

        public OCarouselViewComponent(IStoreContext storeContext,
            IOCarouselModelFactory carouselModelFactory,
            IOCarouselService carouselService,
            OCarouselSettings carouselSettings)
        {
            _storeContex = storeContext;
            _carouselModelFactory = carouselModelFactory;
            _carouselService = carouselService;
            _carouselSettings = carouselSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_carouselSettings.EnableOCarousel)
                return Content("");

            if (!OCarouselHelper.TryGetWidgetZoneId(widgetZone, out int widgetZoneId))
                return Content("");

            var carousels = (await _carouselService.GetAllCarouselsAsync(new List<int> { widgetZoneId }, storeId: (await _storeContex.GetCurrentStoreAsync()).Id, active: true)).ToList();
            if (!carousels.Any())
                return Content("");

            var model = await _carouselModelFactory.PrepareCarouselListModelAsync(carousels);

            return View(model);
        }
    }
}
