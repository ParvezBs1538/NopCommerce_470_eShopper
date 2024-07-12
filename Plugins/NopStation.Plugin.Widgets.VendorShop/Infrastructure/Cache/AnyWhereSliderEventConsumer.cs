using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Infrastructure.Cache
{
    public partial class AnyWhereSliderEventConsumer : IConsumer<EntityInsertedEvent<Slider>>,
        IConsumer<EntityDeletedEvent<Slider>>,
        IConsumer<EntityUpdatedEvent<Slider>>,
        IConsumer<EntityInsertedEvent<SliderItem>>,
        IConsumer<EntityDeletedEvent<SliderItem>>,
        IConsumer<EntityUpdatedEvent<SliderItem>>
    {
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ISliderService _sliderService;

        public AnyWhereSliderEventConsumer(IStaticCacheManager staticCacheManager, ISliderService sliderService)
        {
            _staticCacheManager = staticCacheManager;
            _sliderService = sliderService;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Slider> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix, eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Slider> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix, eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Slider> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix, eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<SliderItem> eventMessage)
        {
            var slider = await _sliderService.GetSliderByIdAsync(eventMessage.Entity.SliderId);
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix, slider.VendorId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<SliderItem> eventMessage)
        {
            var slider = await _sliderService.GetSliderByIdAsync(eventMessage.Entity.SliderId);
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix, slider.VendorId);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<SliderItem> eventMessage)
        {
            var slider = await _sliderService.GetSliderByIdAsync(eventMessage.Entity.SliderId);
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix, slider.VendorId);
        }

    }
}