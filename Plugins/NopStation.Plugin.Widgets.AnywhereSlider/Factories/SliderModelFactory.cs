using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Localization;
using Nop.Services.Media;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Infrastructure.Caching;
using NopStation.Plugin.Widgets.AnywhereSlider.Models;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Factories
{
    public class SliderModelFactory : ISliderModelFactory
    {
        private readonly IPictureService _pictureService;
        private readonly ISliderService _sliderService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly INopStationContext _nopStationContext;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;

        public SliderModelFactory(IPictureService pictureService,
            ISliderService sliderService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            INopStationContext nopStationContext,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager)
        {
            _pictureService = pictureService;
            _sliderService = sliderService;
            _localizationService = localizationService;
            _workContext = workContext;
            _nopStationContext = nopStationContext;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
        }

        protected async Task<string> GetSliderBackgroundImage(Slider slider)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(SliderCacheDefaults.SliderBackgrounPictureKey,
                slider, _storeContext.GetCurrentStoreAsync());

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var pictureUrl = await _pictureService.GetPictureUrlAsync(slider.BackgroundPictureId);
                return pictureUrl;
            });
        }

        public async Task<SliderListModel> PrepareSliderListModelAsync(int widgetZoneId)
        {
            var zoneIds = new List<int> { widgetZoneId };
            var sliders = await _sliderService.GetAllSlidersAsync(widgetZoneIds: zoneIds, storeId: _storeContext.GetCurrentStore().Id, active: true);

            var model = new SliderListModel();
            foreach (var slider in sliders)
            {
                model.Sliders.Add(new SliderListModel.SliderOverviewModel()
                {
                    ShowBackgroundPicture = slider.ShowBackgroundPicture,
                    BackgroundPictureUrl = slider.ShowBackgroundPicture ? await GetSliderBackgroundImage(slider) : "",
                    Id = slider.Id
                });
            }

            return model;
        }

        public async Task<SliderModel> PrepareSliderModelAsync(Slider slider)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(SliderCacheDefaults.SliderModelKey, slider, _nopStationContext.MobileDevice,
                await _workContext.GetWorkingLanguageAsync(), _storeContext.GetCurrentStore(), await _workContext.GetCurrentCustomerAsync());

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var sliderModel = new SliderModel()
                {
                    Id = slider.Id,
                    Name = await _localizationService.GetLocalizedAsync(slider, x => x.Name),
                    WidgetZoneId = slider.WidgetZoneId,
                    Nav = slider.Nav,
                    AutoPlayHoverPause = slider.AutoPlayHoverPause,
                    StartPosition = slider.StartPosition,
                    LazyLoad = slider.LazyLoad,
                    LazyLoadEager = slider.LazyLoadEager,
                    Video = slider.Video,
                    AnimateOut = slider.AnimateOut,
                    AnimateIn = slider.AnimateIn,
                    Loop = slider.Loop,
                    Margin = slider.Margin,
                    AutoPlay = slider.AutoPlay,
                    AutoPlayTimeout = slider.AutoPlayTimeout,
                    RTL = (await _workContext.GetWorkingLanguageAsync()).Rtl
                };

                if (slider.WidgetZoneId != 5)
                    sliderModel.BackGroundPictureUrl = await _pictureService.GetPictureUrlAsync(slider.BackgroundPictureId);

                var sliderItems = await _sliderService.GetSliderItemsBySliderIdAsync(slider.Id);
                foreach (var si in sliderItems)
                {
                    sliderModel.Items.Add(new SliderModel.SliderItemModel()
                    {
                        Id = si.Id,
                        Title = await _localizationService.GetLocalizedAsync(si, x => x.Title),
                        Link = await _localizationService.GetLocalizedAsync(si, x => x.Link),
                        ShopNowLink = await _localizationService.GetLocalizedAsync(si, x => x.ShopNowLink),
                        PictureUrl = await _pictureService.GetPictureUrlAsync(_nopStationContext.MobileDevice ? si.MobilePictureId : si.PictureId),
                        ImageAltText = await _localizationService.GetLocalizedAsync(si, x => x.ImageAltText),
                        ShortDescription = await _localizationService.GetLocalizedAsync(si, x => x.ShortDescription)
                    });
                }

                return sliderModel;
            });
        }
    }
}
