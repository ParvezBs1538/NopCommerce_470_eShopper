using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Services.Media;

namespace NopStation.Plugin.Misc.CloudinaryCdn.Services
{
    public class EventConsumers : IConsumer<EntityInsertedEvent<ProductPicture>>,
        IConsumer<EntityInsertedEvent<Category>>,
        IConsumer<EntityUpdatedEvent<Category>>,

        IConsumer<EntityInsertedEvent<Manufacturer>>,
        IConsumer<EntityUpdatedEvent<Manufacturer>>,

        IConsumer<EntityInsertedEvent<Vendor>>,
        IConsumer<EntityUpdatedEvent<Vendor>>
    {
        private readonly CloudinaryCdnSettings _cloudinaryCdnSettings;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;

        public EventConsumers(CloudinaryCdnSettings cloudinaryCdnSettings,
            IPictureService pictureService,
            MediaSettings mediaSettings)
        {
            _cloudinaryCdnSettings = cloudinaryCdnSettings;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductPicture> eventMessage)
        {
            if (!_cloudinaryCdnSettings.Enabled || CloudinaryCdnHelper.IsAdminArea())
                return;

            if (!_cloudinaryCdnSettings.AutoGenerateRequiredPictures)
                return;

            if (_cloudinaryCdnSettings.AutoGenerateProductThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.ProductThumbPictureSize);

            if (_cloudinaryCdnSettings.AutoGenerateProductDetailsPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.ProductDetailsPictureSize);

            if (_cloudinaryCdnSettings.AutoGenerateProductThumbPictureOnProductDetailsPage)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

            if (_cloudinaryCdnSettings.AutoGenerateAssociatedProductPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.AssociatedProductPictureSize);

            if (_cloudinaryCdnSettings.AutoGenerateCartThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.CartThumbPictureSize);

            if (_cloudinaryCdnSettings.AutoGenerateMiniCartThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.MiniCartThumbPictureSize);

            if (_cloudinaryCdnSettings.AutoGenerateAutoCompleteSearchThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.AutoCompleteSearchThumbPictureSize);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
        {
            if (!_cloudinaryCdnSettings.Enabled || CloudinaryCdnHelper.IsAdminArea())
                return;

            if (!_cloudinaryCdnSettings.AutoGenerateRequiredPictures)
                return;

            if (_cloudinaryCdnSettings.AutoGenerateCategoryThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.CategoryThumbPictureSize);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
        {
            if (!_cloudinaryCdnSettings.Enabled || CloudinaryCdnHelper.IsAdminArea())
                return;

            if (!_cloudinaryCdnSettings.AutoGenerateRequiredPictures)
                return;

            if (_cloudinaryCdnSettings.AutoGenerateCategoryThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.CategoryThumbPictureSize);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Manufacturer> eventMessage)
        {
            if (!_cloudinaryCdnSettings.Enabled || CloudinaryCdnHelper.IsAdminArea())
                return;

            if (!_cloudinaryCdnSettings.AutoGenerateRequiredPictures)
                return;

            if (_cloudinaryCdnSettings.AutoGenerateManufacturerThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.ManufacturerThumbPictureSize);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            if (!_cloudinaryCdnSettings.Enabled || CloudinaryCdnHelper.IsAdminArea())
                return;

            if (!_cloudinaryCdnSettings.AutoGenerateRequiredPictures)
                return;

            if (_cloudinaryCdnSettings.AutoGenerateManufacturerThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.ManufacturerThumbPictureSize);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Vendor> eventMessage)
        {
            if (!_cloudinaryCdnSettings.Enabled || CloudinaryCdnHelper.IsAdminArea())
                return;

            if (!_cloudinaryCdnSettings.AutoGenerateRequiredPictures)
                return;

            if (_cloudinaryCdnSettings.AutoGenerateVendorThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.VendorThumbPictureSize);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Vendor> eventMessage)
        {
            if (!_cloudinaryCdnSettings.Enabled || CloudinaryCdnHelper.IsAdminArea())
                return;

            if (!_cloudinaryCdnSettings.AutoGenerateRequiredPictures)
                return;

            if (_cloudinaryCdnSettings.AutoGenerateVendorThumbPicture)
                await _pictureService.GetPictureUrlAsync(eventMessage.Entity.PictureId, _mediaSettings.VendorThumbPictureSize);
        }
    }
}
