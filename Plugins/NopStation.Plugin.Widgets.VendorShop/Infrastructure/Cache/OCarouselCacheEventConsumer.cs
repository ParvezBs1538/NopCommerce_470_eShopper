using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Infrastructure.Cache
{
    public partial class OCarouselCacheEventConsumer :
        IConsumer<EntityInsertedEvent<OCarousel>>,
        IConsumer<EntityUpdatedEvent<OCarousel>>,
        IConsumer<EntityDeletedEvent<OCarousel>>,
        IConsumer<EntityInsertedEvent<OCarouselItem>>,
        IConsumer<EntityUpdatedEvent<OCarouselItem>>,
        IConsumer<EntityDeletedEvent<OCarouselItem>>,
        IConsumer<EntityInsertedEvent<Product>>,
        IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityDeletedEvent<Product>>
    {
        /// <summary>
        /// Key for caching background picture
        /// </summary>
        /// <remarks>
        /// {0} : vendor id
        /// {1} : carousel id
        /// {2} : store id
        /// </remarks>
        public static CacheKey OCAROUSEL_BACKGROUND_PICTURE_MODEL_KEY = new CacheKey("Nopstation.ocarousel.vendorshop.items.background_picture.{0}-{1}-{2}", OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY);
        public const string OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY = "Nopstation.ocarousel.vendorshop.items.background_picture.{0}";

        public static CacheKey GetOCarouselBackgroundPictureModelKey(int vendorId)
        {
            return new CacheKey("Nopstation.ocarousel.vendorshop.items.background_picture.{0}-{1}-{2}",
                string.Format(OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY, vendorId)
                );
        }

        /// <summary>
        /// Key for caching custom product ids
        /// </summary>
        /// <remarks>
        /// {0} : vendor id
        /// {1} : carousel id
        /// </remarks>
        public static CacheKey OCAROUSEL_CUSTOMRODUCTIDS_MODEL_KEY = new CacheKey("Nopstation.ocarousel.vendorshop.items.customproductids.{0}-{1}", OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY);
        public const string OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY = "Nopstation.ocarousel.vendorshop.items.customproductids.{0}";
        private const string OCAROUSEL_CATEGORIES_PATTERN_KEY = "Nopstation.ocarousel.vendorshop.items.categories.{0}";

        public static CacheKey GetOCarouselCustomProductsModelKey(int vendorId)
        {
            return new CacheKey("Nopstation.ocarousel.vendorshop.items.customproductids.{0}-{1}",
                string.Format(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY, vendorId)
                );
        }

        public static CacheKey GetOCarouselCategoriesModelKey(int vendorId)
        {
            return new CacheKey("Nopstation.ocarousel.vendorshop.items.categories.{0}-{1}",
                string.Format(OCAROUSEL_CATEGORIES_PATTERN_KEY, vendorId)
                );
        }

        private readonly IStaticCacheManager _cacheManager;
        private readonly IOCarouselService _oCarouselService;

        public OCarouselCacheEventConsumer(IStaticCacheManager cacheManager,
            IOCarouselService oCarouselService)
        {
            _cacheManager = cacheManager;
            _oCarouselService = oCarouselService;
        }


        public async Task HandleEventAsync(EntityInsertedEvent<OCarousel> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY, eventMessage.Entity.VendorId);
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY, eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<OCarousel> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY, eventMessage.Entity.VendorId);
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY, eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<OCarousel> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY, eventMessage.Entity.VendorId);
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY, eventMessage.Entity.VendorId);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<OCarouselItem> eventMessage)
        {
            var carousel = await _oCarouselService.GetCarouselByIdAsync(eventMessage.Entity.OCarouselId);
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY, carousel.VendorId);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<OCarouselItem> eventMessage)
        {
            var carousel = await _oCarouselService.GetCarouselByIdAsync(eventMessage.Entity.OCarouselId);
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY, carousel.VendorId);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<OCarouselItem> eventMessage)
        {
            var carousel = await _oCarouselService.GetCarouselByIdAsync(eventMessage.Entity.OCarouselId);
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY, carousel.VendorId);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            if (eventMessage.Entity.VendorId > 0)
            {
                await ClearVendorCategoryCacheAsync(eventMessage.Entity.VendorId);
            }
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
        {
            if (eventMessage.Entity.VendorId > 0)
            {
                await ClearVendorCategoryCacheAsync(eventMessage.Entity.VendorId);
            }
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            if (eventMessage.Entity.VendorId > 0)
            {
                await ClearVendorCategoryCacheAsync(eventMessage.Entity.VendorId);
            }
        }
        async Task ClearVendorCategoryCacheAsync(int vendorId)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(OCAROUSEL_CATEGORIES_PATTERN_KEY), vendorId);
        }
    }
}
