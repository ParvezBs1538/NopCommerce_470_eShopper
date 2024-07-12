using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Infrastructure.Cache
{
    public class CacheEventConsumer :
        IConsumer<EntityUpdatedEvent<ProductPicture>>,
        IConsumer<EntityDeletedEvent<ProductPicture>>,
        IConsumer<EntityInsertedEvent<AjaxFilterSpecificationAttribute>>,
        IConsumer<EntityUpdatedEvent<AjaxFilterSpecificationAttribute>>,
        IConsumer<EntityDeletedEvent<AjaxFilterSpecificationAttribute>>

    {
        private readonly IStaticCacheManager _cacheManager;
        public CacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductPicture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(CategoryImagesModelCache.CategoryProductImagesPrefix, eventMessage.Entity.ProductId));

        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductPicture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(CategoryImagesModelCache.CategoryProductImagesPrefix, eventMessage.Entity.ProductId));
        }

        public async Task HandleEventAsync(EntityInsertedEvent<AjaxFilterSpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AjaxFilterDefaults.AjaxFilterAvailableSpecificationAttributesPrefix);

        }

        public async Task HandleEventAsync(EntityUpdatedEvent<AjaxFilterSpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AjaxFilterDefaults.AjaxFilterAvailableSpecificationAttributesPrefix);

        }

        public async Task HandleEventAsync(EntityDeletedEvent<AjaxFilterSpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AjaxFilterDefaults.AjaxFilterAvailableSpecificationAttributesPrefix);

        }
    }
}
