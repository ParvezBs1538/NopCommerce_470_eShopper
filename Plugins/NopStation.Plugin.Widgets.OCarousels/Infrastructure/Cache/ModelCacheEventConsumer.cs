using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Infrastructure.Cache
{
    public partial class ModelCacheEventConsumer :
        IConsumer<EntityInsertedEvent<Manufacturer>>,
        IConsumer<EntityUpdatedEvent<Manufacturer>>,
        IConsumer<EntityDeletedEvent<Manufacturer>>,
        IConsumer<EntityInsertedEvent<Category>>,
        IConsumer<EntityUpdatedEvent<Category>>,
        IConsumer<EntityDeletedEvent<Category>>,
        IConsumer<EntityInsertedEvent<OCarousel>>,
        IConsumer<EntityUpdatedEvent<OCarousel>>,
        IConsumer<EntityDeletedEvent<OCarousel>>,
        IConsumer<EntityInsertedEvent<OCarouselItem>>,
        IConsumer<EntityUpdatedEvent<OCarouselItem>>,
        IConsumer<EntityDeletedEvent<OCarouselItem>>
    {
        /// <summary>
        /// Key for caching background picture
        /// </summary>
        /// <remarks>
        /// {0} : carousel id
        /// {1} : store id
        /// </remarks>
        public static CacheKey OCAROUSEL_BACKGROUND_PICTURE_MODEL_KEY = new CacheKey("Nopstation.ocarousel.items.background_picture.{0}-{1}", OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY);
        public const string OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY = "Nopstation.ocarousel.items.background_picture.";

        /// <summary>
        /// Key for caching category
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : customer roles
        /// {2} : language id
        /// {3} : store id
        /// </remarks>
        public static CacheKey OCAROUSEL_CATEGORIES_MODEL_KEY = new CacheKey("Nopstation.ocarousel.items.categories.{0}-{1}-{2}-{3}", OCAROUSEL_CATEGORIES_PATTERN_KEY);
        public const string OCAROUSEL_CATEGORIES_PATTERN_KEY = "Nopstation.ocarousel.items.categories.";

        /// <summary>
        /// Key for caching manufacturer
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer id
        /// {1} : customer roles
        /// {2} : language id
        /// {3} : store id
        /// </remarks>
        public static CacheKey OCAROUSEL_MANUFACTURERS_MODEL_KEY = new CacheKey("Nopstation.ocarousel.items.manufacturers.{0}-{1}-{2}-{3}", OCAROUSEL_MANUFACTURERS_PATTERN_KEY);
        public const string OCAROUSEL_MANUFACTURERS_PATTERN_KEY = "Nopstation.ocarousel.items.manufacturers.";

        /// <summary>
        /// Key for caching custom product ids
        /// </summary>
        /// <remarks>
        /// {0} : carousel id
        /// </remarks>
        public static CacheKey OCAROUSEL_CUSTOMRODUCTIDS_MODEL_KEY = new CacheKey("Nopstation.ocarousel.items.customproductids.{0}", OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY);
        public const string OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY = "Nopstation.ocarousel.items.customproductids.";

        private readonly IStaticCacheManager _cacheManager;

        public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Manufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_MANUFACTURERS_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_MANUFACTURERS_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_MANUFACTURERS_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CATEGORIES_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CATEGORIES_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CATEGORIES_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<OCarousel> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<OCarousel> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<OCarousel> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_BACKGROUND_PICTURE_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<OCarouselItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<OCarouselItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<OCarouselItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_CUSTOMPRODUCTIDS_PATTERN_KEY);
        }
    }
}
