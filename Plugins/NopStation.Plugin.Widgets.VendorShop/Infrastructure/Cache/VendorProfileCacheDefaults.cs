using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Infrastructure.Cache
{
    public class VendorProfileCacheDefaults :
        IConsumer<EntityUpdatedEvent<VendorProfile>>,
        IConsumer<EntityInsertedEvent<VendorProfile>>,
        IConsumer<EntityDeletedEvent<VendorProfile>>
    {
        private readonly IStaticCacheManager _staticCacheManager;

        public static CacheKey VendorProfileCacheKey => new("Nopstation.vendorshop.vendorprofile.{0}-{1}", VendorProfilePrefix);
        public static string VendorProfilePrefix => "Nopstation.vendorshop.vendorprofile.{0}";

        public static CacheKey GetVendorProfileCacheKey(int vendorId, int storeId = 0)
        {
            return new CacheKey(string.Format("Nopstation.vendorshop.vendorprofile.{0}-{1}", vendorId, storeId),
                string.Format(VendorProfilePrefix, vendorId)
                );
        }

        public VendorProfileCacheDefaults(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<VendorProfile> eventMessage)
        {
            await ClearProfileCache(eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<VendorProfile> eventMessage)
        {
            await ClearProfileCache(eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<VendorProfile> eventMessage)
        {
            await ClearProfileCache(eventMessage.Entity.VendorId);
        }
        private async Task ClearProfileCache(int vendorId)
        {
            await _staticCacheManager.RemoveByPrefixAsync(string.Format(VendorProfilePrefix, vendorId));
        }
    }
}
