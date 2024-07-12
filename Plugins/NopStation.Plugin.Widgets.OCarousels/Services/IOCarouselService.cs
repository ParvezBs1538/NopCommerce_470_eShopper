using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Services
{
    public interface IOCarouselService
    {
        Task<IPagedList<OCarousel>> GetAllCarouselsAsync(List<int> widgetZoneIds = null, List<int> dataSources = null,
            int storeId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue);

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
    }
}