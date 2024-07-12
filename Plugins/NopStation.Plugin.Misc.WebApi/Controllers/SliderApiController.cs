using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Factories;
using NopStation.Plugin.Misc.WebApi.Models.Sliders;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/slider")]
    public class SliderApiController : BaseApiController
    {
        #region Field

        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ISliderModelFactory _sliderModelFactory;

        #endregion

        #region Ctor

        public SliderApiController(IStoreService storeService,
            IWorkContext workContext,
            ISliderModelFactory sliderModelFactory)
        {
            _storeService = storeService;
            _workContext = workContext;
            _sliderModelFactory = sliderModelFactory;
        }

        #endregion

        #region Action Method

        [HttpGet("homepageslider")]
        public async Task<IActionResult> HomePageSlider()
        {
            var response = new GenericResponseModel<HomePageSliderModel>();
            response.Data = await _sliderModelFactory.PrepareHomePageSliderModelAsync();
            return Ok(response);
        }

        #endregion
    }
}
