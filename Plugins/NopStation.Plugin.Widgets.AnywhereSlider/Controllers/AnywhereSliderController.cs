using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.AnywhereSlider.Factories;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Controllers
{
    public class AnywhereSliderController : NopStationPublicController
    {
        private readonly ISliderModelFactory _sliderModelFactory;
        private readonly ISliderService _sliderService;

        public AnywhereSliderController(ISliderModelFactory sliderModelFactory,
            ISliderService sliderService)
        {
            _sliderModelFactory = sliderModelFactory;
            _sliderService = sliderService;
        }

        [HttpPost]
        public async Task<IActionResult> Details(int sliderId)
        {
            var slider = await _sliderService.GetSliderByIdAsync(sliderId);
            if (slider == null || slider.Deleted || !slider.Active)
                return Json(new { result = false });

            var model = await _sliderModelFactory.PrepareSliderModelAsync(slider);
            var html = await RenderPartialViewToStringAsync("Details", model);

            return Json(new { result = true, html = html, sliderId = sliderId });
        }
    }
}
