using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Media;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Cache;
using NopStation.Plugin.Misc.WebApi.Models.Sliders;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Factories
{
    public class SliderModelFactory : ISliderModelFactory
    {
        private readonly IApiSliderService _sliderService;
        private readonly WebApiSettings _webApiSettings;
        private readonly IPictureService _pictureService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWorkContext _workContext;

        public SliderModelFactory(IApiSliderService sliderService,
            WebApiSettings webApiSettings,
            IPictureService pictureService,
            IStaticCacheManager cacheManager,
            IWorkContext workContext)
        {
            _sliderService = sliderService;
            _webApiSettings = webApiSettings;
            _pictureService = pictureService;
            _cacheManager = cacheManager;
            _workContext = workContext;
        }

        public virtual async Task<HomePageSliderModel> PrepareHomePageSliderModelAsync()
        {
            var maxSliders = _webApiSettings.MaximumNumberOfHomePageSliders > 0 ? _webApiSettings.MaximumNumberOfHomePageSliders : int.MaxValue;

            var key = string.Format(ApiModelCacheDefaults.SliderModelKey, (await _workContext.GetCurrentCustomerAsync()).Id, maxSliders, (await _workContext.GetWorkingLanguageAsync()).Id, _webApiSettings.ShowHomepageSlider);
            var cacheKey = new CacheKey(key);
            var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new HomePageSliderModel();
                if (!_webApiSettings.ShowHomepageSlider)
                    return model;

                var sliders = await _sliderService.GetActiveApiSlidersAsync(maxSliders);

                if (!sliders.Any())
                    return model;

                model.IsEnabled = true;
                foreach (var slider in sliders)
                {
                    model.Sliders.Add(new HomePageSliderModel.SliderModel()
                    {
                        EntityId = slider.EntityId,
                        Id = slider.Id,
                        ImageUrl = await _pictureService.GetPictureUrlAsync(slider.PictureId),
                        SliderType = (int)slider.SliderType
                    });
                }

                return model;
            });

            return cachedModel;
        }
    }
}
