using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Infrastructure.Cache
{
    public class ModelCacheEventConsumer :
        IConsumer<EntityInsertedEvent<ProductTab>>,
        IConsumer<EntityUpdatedEvent<ProductTab>>,
        IConsumer<EntityDeletedEvent<ProductTab>>,
        IConsumer<EntityInsertedEvent<ProductTabItem>>,
        IConsumer<EntityUpdatedEvent<ProductTabItem>>,
        IConsumer<EntityDeletedEvent<ProductTabItem>>,
        IConsumer<EntityInsertedEvent<ProductTabItemProduct>>,
        IConsumer<EntityUpdatedEvent<ProductTabItemProduct>>,
        IConsumer<EntityDeletedEvent<ProductTabItemProduct>>
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : widget zone id
        /// {1} : customer role ids
        /// {2} : language id
        /// {3} : store id
        /// </remarks>
        public static CacheKey PRODUCT_TAB_MODEL_KEY = new CacheKey("Admin.NopStation.ProductTabs.producttab-{0}-{1}-{2}-{3}", PRODUCT_TAB_PATTERN_KEY);
        public const string PRODUCT_TAB_PATTERN_KEY = "Admin.NopStation.ProductTabs.producttab";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product tab id
        /// {1} : customer role ids
        /// {2} : language id
        /// {3} : store id
        /// </remarks>
        public static CacheKey PRODUCT_TAB_ITEM_MODEL_KEY = new CacheKey("Admin.NopStation.ProductTabs.producttab.item-{0}-{1}-{2}-{3}", PRODUCT_TAB_ITEM_PATTERN_KEY);
        public const string PRODUCT_TAB_ITEM_PATTERN_KEY = "Admin.NopStation.ProductTabs.producttab.item";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product tab item id
        /// {1} : customer role ids
        /// {2} : language id
        /// {3} : store id
        /// </remarks>
        public static CacheKey PRODUCT_TAB_ITEM_PRODUCT_MODEL_KEY = new CacheKey("Admin.NopStation.ProductTabs.producttab.item.products-{0}-{1}-{2}-{3}", PRODUCT_TAB_ITEM_PRODUCT_PATTERN_KEY);
        public const string PRODUCT_TAB_ITEM_PRODUCT_PATTERN_KEY = "Admin.NopStation.ProductTabs.producttab.item.products";

        private readonly IStaticCacheManager _cacheManager;

        public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductTab> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTab> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductTab> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductTabItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_ITEM_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTabItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_ITEM_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductTabItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_ITEM_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductTabItemProduct> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_ITEM_PRODUCT_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTabItemProduct> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_ITEM_PRODUCT_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductTabItemProduct> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_ITEM_PRODUCT_PATTERN_KEY);
        }
    }
}
