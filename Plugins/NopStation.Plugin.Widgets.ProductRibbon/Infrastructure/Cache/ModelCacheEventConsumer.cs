using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Events;

namespace NopStation.Plugin.Widgets.ProductRibbon.Infrastructure.Cache
{
    public class ModelCacheEventConsumer :
        IConsumer<EntityInsertedEvent<Product>>,
        IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityDeletedEvent<Product>>
    {
        public static CacheKey PRODUCT_RIBBON_MODEL_KEY = new CacheKey("Nopstation.productribbon.{0}-{1}-{2}", PRODUCT_RIBBON_PATTERN_KEY);
        public const string PRODUCT_RIBBON_PATTERN_KEY = "Nopstation.productribbon.";

        private readonly IStaticCacheManager _staticCacheManager;

        public ModelCacheEventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(PRODUCT_RIBBON_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(PRODUCT_RIBBON_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(PRODUCT_RIBBON_PATTERN_KEY);
        }
    }
}
