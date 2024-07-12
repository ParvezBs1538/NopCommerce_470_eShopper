using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Services;

public interface ISmartDealCarouselService
{
    #region Carousels

    Task<IPagedList<SmartDealCarousel>> GetAllCarouselsAsync(string keywords = null, int storeId = 0, int productId = 0,
        bool overrideProduct = false, bool showHidden = false, bool? overridePublished = null, bool validScheduleOnly = false,
        int productSourceTypeId = 0, string widgetZone = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<SmartDealCarousel> GetCarouselByIdAsync(int carouselId);

    Task InsertCarouselAsync(SmartDealCarousel carousel);

    Task UpdateCarouselAsync(SmartDealCarousel carousel);

    Task DeleteCarouselAsync(SmartDealCarousel carousel, bool deleteReletedData = true);

    #endregion

    #region Carousel products

    Task<IList<SmartDealCarouselProductMapping>> GetCarouselProductMappingsByCarouselIdAsync(int carouselId);

    Task<IPagedList<Product>> GetProductsWithAppliedDiscountAsync(Discount discount,
        bool showHidden = false, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<IList<Product>> GetProductsByCarouselIdAsync(int carouselId, int storeId = 0, int recordsToReturn = 0, bool activeOnly = true);

    Task<SmartDealCarouselProductMapping> GetCarouselProductMappingAsync(int carouselId, int productId);

    Task<SmartDealCarouselProductMapping> GetCarouselProductMappingByIdAsync(int carouselProductMappingId);

    Task InsertCarouselProductMappingAsync(SmartDealCarouselProductMapping carouselProductMapping);

    Task UpdateCarouselProductMappingAsync(SmartDealCarouselProductMapping carouselProductMapping);

    Task DeleteCarouselProductMappingAsync(SmartDealCarouselProductMapping carouselProductMapping);

    #endregion
}