using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Events;

namespace NopStation.Plugin.Widgets.PictureZoom.Infrastructure.Cache;

public partial class ModelCacheEventConsumer :
    IConsumer<EntityInsertedEvent<Product>>,
    IConsumer<EntityUpdatedEvent<Product>>,
    IConsumer<EntityDeletedEvent<Product>>
{
    public static string PrictureZoom_model_key = "Nopstation.pricturezoom.{0}-{1}-{2}";
    public static string PrictureZoom_patern_key = "Nopstation.pricturezoom.";

    private readonly IStaticCacheManager _cacheManager;

    public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(PrictureZoom_patern_key);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(PrictureZoom_patern_key);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(PrictureZoom_patern_key);
    }
}
