using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.VendorShop.Factories;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Controllers
{
    public class OCarouselVendorShopController : NopStationPublicController
    {
        private readonly IOCarouselModelFactory _carouselModelFactory;
        private readonly IOCarouselService _carouselService;

        public OCarouselVendorShopController(IOCarouselModelFactory carouselModelFactory,
            IOCarouselService carouselService)
        {
            _carouselModelFactory = carouselModelFactory;
            _carouselService = carouselService;
        }

        [HttpPost]
        public async Task<IActionResult> Details(int carouselId)
        {
            var carousel = await _carouselService.GetCarouselByIdAsync(carouselId);
            if (carousel == null || carousel.Deleted || !carousel.Active)
                return Json(new { result = false });

            var model = await _carouselModelFactory.PrepareCarouselModelAsync(carousel);
            var html = await RenderPartialViewToStringAsync(model.CarouselType.ToString(), model);

            return Json(new { result = true, html, carouselid = carouselId });
        }
    }
}
