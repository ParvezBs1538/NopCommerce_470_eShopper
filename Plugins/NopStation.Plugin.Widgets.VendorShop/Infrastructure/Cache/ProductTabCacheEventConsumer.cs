using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Infrastructure.Cache
{
    public class ProductTabCacheEventConsumer :
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
        public const string PRODUCT_TAB_PATTERN_KEY = "Admin.NopStation.VendorShop.ProductTabs.{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : vendor id
        /// {1} : widget zone id
        /// {2} : customer role ids
        /// {3} : language id
        /// {4} : store id
        /// </remarks>
        public static CacheKey PRODUCT_TAB_MODEL_KEY = new("Admin.NopStation.VendorShop.ProductTabs.producttab-{0}-{1}-{2}-{3}-{4}", PRODUCT_TAB_PATTERN_KEY);

        public static CacheKey GetProductTabModelKey(int vendorId)
        {
            return new CacheKey("Admin.NopStation.VendorShop.ProductTabs.producttab-{0}-{1}-{2}-{3}-{4}",
                string.Format(PRODUCT_TAB_PATTERN_KEY, vendorId)
                );
        }

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : vendor id
        /// {1} : product tab id
        /// {2} : customer role ids
        /// {3} : language id
        /// {4} : store id
        /// </remarks>
        public static CacheKey PRODUCT_TAB_ITEM_MODEL_KEY = new("Admin.NopStation.VendorShop.ProductTabs.producttab.item-{0}-{1}-{2}-{3}-{4}", PRODUCT_TAB_PATTERN_KEY);

        public static CacheKey GetProductTabItemModelKey(int vendorId)
        {
            return new CacheKey("Admin.NopStation.VendorShop.ProductTabs.producttab.item-{0}-{1}-{2}-{3}-{4}",
                string.Format(PRODUCT_TAB_PATTERN_KEY, vendorId)
                );
        }

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : vendor id
        /// {1} : product tab item id
        /// {2} : customer role ids
        /// {3} : language id
        /// {4} : store id
        /// </remarks>
        public static CacheKey PRODUCT_TAB_ITEM_PRODUCT_MODEL_KEY = new("Admin.NopStation.VendorShop.ProductTabs.producttab.item.products-{0}-{1}-{2}-{3}-{4}", PRODUCT_TAB_PATTERN_KEY);

        public static CacheKey GetProductTabItemProductModelKey(int vendorId)
        {
            var prefix = string.Format(PRODUCT_TAB_PATTERN_KEY, vendorId);
            return new CacheKey("Admin.NopStation.VendorShop.ProductTabs.producttab.item.products-{0}-{1}-{2}-{3}-{4}",
                prefix
                );
        }

        private readonly IStaticCacheManager _cacheManager;
        private readonly IProductTabService _productTabService;

        public ProductTabCacheEventConsumer(IStaticCacheManager cacheManager,
            IProductTabService productTabService)
        {
            _cacheManager = cacheManager;
            _productTabService = productTabService;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductTab> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY, eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTab> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY, eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductTab> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY, eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductTabItem> eventMessage)
        {
            var productTab = await _productTabService.GetProductTabByIdAsync(eventMessage.Entity.ProductTabId);
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY, productTab.VendorId);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTabItem> eventMessage)
        {
            var productTab = await _productTabService.GetProductTabByIdAsync(eventMessage.Entity.ProductTabId);
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY, productTab.VendorId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductTabItem> eventMessage)
        {
            var productTab = await _productTabService.GetProductTabByIdAsync(eventMessage.Entity.ProductTabId);
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY, productTab.VendorId);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductTabItemProduct> eventMessage)
        {
            var productTabItem = await _productTabService.GetProductTabItemByIdAsync(eventMessage.Entity.ProductTabItemId);
            var productTab = await _productTabService.GetProductTabByIdAsync(productTabItem.ProductTabId);
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY, productTab.VendorId);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTabItemProduct> eventMessage)
        {
            var productTabItem = await _productTabService.GetProductTabItemByIdAsync(eventMessage.Entity.ProductTabItemId);
            var productTab = await _productTabService.GetProductTabByIdAsync(productTabItem.ProductTabId);
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY, productTab.VendorId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductTabItemProduct> eventMessage)
        {
            var productTabItem = await _productTabService.GetProductTabItemByIdAsync(eventMessage.Entity.ProductTabItemId);
            var productTab = await _productTabService.GetProductTabByIdAsync(productTabItem.ProductTabId);
            await _cacheManager.RemoveByPrefixAsync(PRODUCT_TAB_PATTERN_KEY, productTab.VendorId);
        }
    }
}
