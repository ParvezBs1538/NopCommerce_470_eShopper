using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Infrastructure.Cache
{
    public partial class ProductQACacheEventConsumer :
        IConsumer<EntityInsertedEvent<Domains.ProductQnA>>,
        IConsumer<EntityUpdatedEvent<Domains.ProductQnA>>,
        IConsumer<EntityDeletedEvent<Domains.ProductQnA>>
    {
        private readonly IStaticCacheManager _cacheManager;

        public ProductQACacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        public async Task HandleEventAsync(EntityInsertedEvent<Domains.ProductQnA> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ProductQACache.ProductsQAPrefixCacheKey);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Domains.ProductQnA> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ProductQACache.ProductsQAPrefixCacheKey);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Domains.ProductQnA> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ProductQACache.ProductsQAPrefixCacheKey);
        }
    }
}