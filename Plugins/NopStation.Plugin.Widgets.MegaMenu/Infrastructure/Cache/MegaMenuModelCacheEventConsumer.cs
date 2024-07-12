using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Topics;
using Nop.Core.Events;
using Nop.Services.Events;

namespace NopStation.Plugin.Widgets.MegaMenu.Infrastructure.Cache;

public class MegaMenuModelCacheEventConsumer :
    IConsumer<EntityInsertedEvent<Manufacturer>>,
    IConsumer<EntityUpdatedEvent<Manufacturer>>,
    IConsumer<EntityDeletedEvent<Manufacturer>>,
    IConsumer<EntityInsertedEvent<Category>>,
    IConsumer<EntityUpdatedEvent<Category>>,
    IConsumer<EntityDeletedEvent<Category>>,
    IConsumer<EntityInsertedEvent<Topic>>,
    IConsumer<EntityUpdatedEvent<Topic>>,
    IConsumer<EntityDeletedEvent<Topic>>,
    IConsumer<EntityInsertedEvent<Setting>>,
    IConsumer<EntityUpdatedEvent<Setting>>,
    IConsumer<EntityDeletedEvent<Setting>>
{
    /// <summary>
    /// Key for caching
    /// </summary>
    /// <remarks>
    /// {0} : language id
    /// {1} : customer role ids
    /// {2} : store id
    /// </remarks>
    public static CacheKey MEGAMENU_TOPICS_MODEL_KEY = new CacheKey("Nopstation.megamenu.topic-{0}-{1}-{2}", MEGAMENU_PATERN_KEY);

    /// <summary>
    /// Key for caching
    /// </summary>
    /// <remarks>
    /// {0} : language id
    /// {1} : customer role ids
    /// {2} : store id
    /// </remarks>
    public static CacheKey MEGAMENU_CATEGORIES_MODEL_KEY = new CacheKey("Nopstation.megamenu.categories-{0}-{1}-{2}", MEGAMENU_PATERN_KEY);

    /// <summary>
    /// Key for caching
    /// </summary>
    /// <remarks>
    /// {0} : language id
    /// {1} : customer role ids
    /// {2} : store id
    /// </remarks>
    public static CacheKey MEGAMENU_MANUFACTURERS_MODEL_KEY = new CacheKey("Nopstation.megamenu.manufacturers-{0}-{1}-{2}", MEGAMENU_PATERN_KEY);

    /// <summary>
    /// Key for caching
    /// </summary>
    /// <remarks>
    /// {0} : language id
    /// {1} : customer role ids
    /// {2} : store id
    /// </remarks>
    public static CacheKey MEGAMENU_MODEL_KEY = new CacheKey("Nopstation.megamenu.all-{0}-{1}-{2}", MEGAMENU_PATERN_KEY);
    public const string MEGAMENU_PATERN_KEY = "Nopstation.megamenu.";

    private readonly IStaticCacheManager _staticCacheManager;

    public MegaMenuModelCacheEventConsumer(IStaticCacheManager staticCacheManager)
    {
        _staticCacheManager = staticCacheManager;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<Manufacturer> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }
    public async Task HandleEventAsync(EntityInsertedEvent<Topic> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Topic> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Topic> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<Setting> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Setting> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Setting> eventMessage)
    {
        await _staticCacheManager.RemoveByPrefixAsync(MEGAMENU_PATERN_KEY);
    }
}
