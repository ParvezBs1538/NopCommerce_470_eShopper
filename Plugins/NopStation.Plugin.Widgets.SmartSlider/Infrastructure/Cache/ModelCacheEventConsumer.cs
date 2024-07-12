using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Infrastructure.Cache;

public partial class ModelCacheEventConsumer :
    IConsumer<EntityInsertedEvent<SmartSlider>>,
    IConsumer<EntityUpdatedEvent<SmartSlider>>,
    IConsumer<EntityDeletedEvent<SmartSlider>>
{
    private readonly IStaticCacheManager _cacheManager;

    public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<SmartSlider> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.SliderModelPrefix, eventMessage.Entity.Id);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<SmartSlider> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.SliderModelPrefix, eventMessage.Entity.Id);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<SmartSlider> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.SliderModelPrefix, eventMessage.Entity.Id);
    }
}
