using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Events;

namespace NopStation.Plugin.Widgets.VendorShop.Services.Cache
{
    public class VendorCategorycacheEventConsumer : IConsumer<EntityInsertedEvent<Product>>,
         IConsumer<EntityUpdatedEvent<Product>>,
         IConsumer<EntityDeletedEvent<Product>>
    {
        private readonly IStaticCacheManager _staticCacheManager;

        public VendorCategorycacheEventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            if (eventMessage.Entity.VendorId > 0)
            {
                await ClearVendorcategoryCacheAsync(eventMessage.Entity.VendorId);
            }
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
        {
            if (eventMessage.Entity.VendorId > 0)
            {
                await ClearVendorcategoryCacheAsync(eventMessage.Entity.VendorId);
            }
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            if (eventMessage.Entity.VendorId > 0)
            {
                await ClearVendorcategoryCacheAsync(eventMessage.Entity.VendorId);
            }
        }
        private async Task ClearVendorcategoryCacheAsync(int vendorId)
        {
            await _staticCacheManager.RemoveByPrefixAsync(VendorCategoryFilterCacheDefault.GetAllVendorCategoryCacheKeyPrefix(vendorId));
        }
    }
}
