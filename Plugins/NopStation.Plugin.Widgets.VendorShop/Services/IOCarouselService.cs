using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public interface IOCarouselService
    {
        Task<IPagedList<OCarousel>> GetAllCarouselsAsync(List<int> widgetZoneIds = null, List<int> dataSources = null,
            int storeId = 0, int vendorId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<OCarousel> GetCarouselByIdAsync(int carouselId);

        Task InsertCarouselAsync(OCarousel oCarousel);

        Task UpdateCarouselAsync(OCarousel oCarousel);

        Task DeleteCarouselAsync(OCarousel oCarousel);

        Task<IPagedList<OCarouselItem>> GetOCarouselItemsByOCarouselIdAsync(int carouselId, int pageIndex = 0,
            int pageSize = int.MaxValue);

        Task<OCarouselItem> GetOCarouselItemByIdAsync(int carouselItemId);

        Task InsertOCarouselItemAsync(OCarouselItem carouselItem);

        Task UpdateOCarouselItemAsync(OCarouselItem carouselItem);

        Task DeleteOCarouselItemAsync(OCarouselItem carouselItem);

        Task<IPagedList<Product>> GetProductsMarkedAsNewAsync(int vendorId, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}