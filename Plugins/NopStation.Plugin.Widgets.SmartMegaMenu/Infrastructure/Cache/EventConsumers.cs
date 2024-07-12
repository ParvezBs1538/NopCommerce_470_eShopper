using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Infrastructure.Cache;

public class EventConsumers : IConsumer<EntityUpdatedEvent<MegaMenuItem>>,
    IConsumer<EntityInsertedEvent<MegaMenuItem>>,
    IConsumer<EntityDeletedEvent<MegaMenuItem>>
{
    private readonly IStaticCacheManager _staticCacheManager;

    public EventConsumers(IStaticCacheManager staticCacheManager)
    {
        _staticCacheManager = staticCacheManager;
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<MegaMenuItem> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.MegaMenuItemPicturePrefix, eventMessage.Entity.Id);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<MegaMenuItem> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.MegaMenuItemPicturePrefix, eventMessage.Entity.Id);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<MegaMenuItem> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.MegaMenuItemPicturePrefix, eventMessage.Entity.Id);
    }
}
