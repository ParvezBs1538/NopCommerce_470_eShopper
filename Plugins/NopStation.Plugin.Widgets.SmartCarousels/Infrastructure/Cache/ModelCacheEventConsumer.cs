using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Infrastructure.Cache;

public partial class ModelCacheEventConsumer :
    IConsumer<EntityUpdatedEvent<Manufacturer>>,
    IConsumer<EntityDeletedEvent<Manufacturer>>,
    IConsumer<EntityUpdatedEvent<Category>>,
    IConsumer<EntityDeletedEvent<Category>>,
    IConsumer<EntityUpdatedEvent<Vendor>>,
    IConsumer<EntityDeletedEvent<Vendor>>,

    IConsumer<EntityInsertedEvent<SmartCarouselManufacturerMapping>>,
    IConsumer<EntityUpdatedEvent<SmartCarouselManufacturerMapping>>,
    IConsumer<EntityDeletedEvent<SmartCarouselManufacturerMapping>>,
    IConsumer<EntityInsertedEvent<SmartCarouselCategoryMapping>>,
    IConsumer<EntityUpdatedEvent<SmartCarouselCategoryMapping>>,
    IConsumer<EntityDeletedEvent<SmartCarouselCategoryMapping>>,
    IConsumer<EntityInsertedEvent<SmartCarouselVendorMapping>>,
    IConsumer<EntityUpdatedEvent<SmartCarouselVendorMapping>>,
    IConsumer<EntityDeletedEvent<SmartCarouselVendorMapping>>,
    IConsumer<EntityInsertedEvent<SmartCarouselPictureMapping>>,
    IConsumer<EntityUpdatedEvent<SmartCarouselPictureMapping>>,
    IConsumer<EntityDeletedEvent<SmartCarouselPictureMapping>>,

    IConsumer<EntityInsertedEvent<SmartCarousel>>,
    IConsumer<EntityUpdatedEvent<SmartCarousel>>,
    IConsumer<EntityDeletedEvent<SmartCarousel>>
{
    private readonly IStaticCacheManager _cacheManager;

    public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.ManufacturerListModelPrefix);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.ManufacturerListModelPrefix);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselCategoryListModelPrefix);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselCategoryListModelPrefix);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Vendor> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.VendorListModelPrefix);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Vendor> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.VendorListModelPrefix);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<SmartCarouselManufacturerMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselManufacturerListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<SmartCarouselManufacturerMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselManufacturerListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<SmartCarouselManufacturerMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselManufacturerListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<SmartCarouselCategoryMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselCategoryListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<SmartCarouselCategoryMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselCategoryListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<SmartCarouselCategoryMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselCategoryListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<SmartCarouselVendorMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselVendorListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<SmartCarouselVendorMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselVendorListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<SmartCarouselVendorMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselVendorListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<SmartCarouselPictureMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselPictureListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<SmartCarouselPictureMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselPictureListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<SmartCarouselPictureMapping> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselPictureListModelPrefix, eventMessage.Entity.CarouselId);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<SmartCarousel> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselModelPrefix, eventMessage.Entity.Id);
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselByWidgetZoneKeyPrefix);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<SmartCarousel> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselModelPrefix, eventMessage.Entity.Id);
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselByWidgetZoneKeyPrefix);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<SmartCarousel> eventMessage)
    {
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselModelPrefix, eventMessage.Entity.Id);
        await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CarouselByWidgetZoneKeyPrefix);
    }
}
