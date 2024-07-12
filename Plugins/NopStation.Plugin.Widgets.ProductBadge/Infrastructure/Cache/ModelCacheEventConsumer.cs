//using System.Threading.Tasks;
//using Nop.Core.Caching;
//using Nop.Core.Domain.Catalog;
//using Nop.Core.Domain.Orders;
//using Nop.Core.Events;
//using Nop.Services.Events;

//namespace NopStation.Plugin.Widgets.ProductBadge.Infrastructure.Cache
//{
//    public partial class ModelCacheEventConsumer : IConsumer<EntityUpdatedEvent<Product>>,
//        IConsumer<EntityDeletedEvent<Product>>,
//        IConsumer<EntityInsertedEvent<Order>>,
//        IConsumer<EntityUpdatedEvent<Order>>,
//        IConsumer<EntityDeletedEvent<Order>>,
//        IConsumer<EntityInsertedEvent<Product>>
//    {
//        #region Fields

//        private readonly IStaticCacheManager _staticCacheManager;

//        #endregion Fields

//        #region Ctor

//        public ModelCacheEventConsumer(IStaticCacheManager staticCacheManager)
//        {
//            _staticCacheManager = staticCacheManager;
//        }

//        #endregion

//        #region Methods

//        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
//        {
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerProductPrefixCacheKey);
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerPrefixCacheKey);
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.Smart_Product_badge_pattern_key);
//        }

//        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
//        {
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerProductPrefixCacheKey);
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerPrefixCacheKey);
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.Smart_Product_badge_pattern_key);
//        }

//        public async Task HandleEventAsync(EntityInsertedEvent<Order> eventMessage)
//        {
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerProductPrefixCacheKey);
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerPrefixCacheKey);
//        }

//        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
//        {
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerProductPrefixCacheKey);
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerPrefixCacheKey);
//        }

//        public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
//        {
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerProductPrefixCacheKey);
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.BestSellerPrefixCacheKey);
//        }

//        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
//        {
//            await _staticCacheManager.RemoveByPrefixAsync(CacheDefaults.Smart_Product_badge_pattern_key);
//        }

//        #endregion
//    }
//}