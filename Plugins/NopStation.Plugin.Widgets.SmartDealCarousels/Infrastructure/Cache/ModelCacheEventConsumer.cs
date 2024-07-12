using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Infrastructure.Cache;

public partial class ModelCacheEventConsumer :
    IConsumer<EntityInsertedEvent<SmartDealCarousel>>,
    IConsumer<EntityUpdatedEvent<SmartDealCarousel>>,
    IConsumer<EntityDeletedEvent<SmartDealCarousel>>
{
    private readonly IStaticCacheManager _cacheManager;

    public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<SmartDealCarousel> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselModelPrefix, eventMessage.Entity.Id);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<SmartDealCarousel> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselModelPrefix, eventMessage.Entity.Id);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<SmartDealCarousel> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselModelPrefix, eventMessage.Entity.Id);
    }
}
