using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Infrastructure.Caching
{
    public partial class EventConsumer : IConsumer<EntityInsertedEvent<Slider>>,
        IConsumer<EntityDeletedEvent<Slider>>,
        IConsumer<EntityUpdatedEvent<Slider>>,
        IConsumer<EntityInsertedEvent<SliderItem>>,
        IConsumer<EntityDeletedEvent<SliderItem>>,
        IConsumer<EntityUpdatedEvent<SliderItem>>
    {
        private readonly IStaticCacheManager _staticCacheManager;

        public EventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Slider> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Slider> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Slider> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<SliderItem> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<SliderItem> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<SliderItem> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(SliderCacheDefaults.SliderModelPrefix);
        }
    }
}