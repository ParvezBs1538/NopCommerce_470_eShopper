using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services;

public interface ISmartCarouselService
{
    #region Carousels

    Task<IPagedList<SmartCarousel>> GetAllCarouselsAsync(string keywords = null, int storeId = 0,
        int productId = 0, bool overrideProduct = false, bool showHidden = false, bool? overridePublished = null,
        bool validScheduleOnly = false, int carouselTypeId = 0, int productSourceTypeId = 0, string widgetZone = null,
        int pageIndex = 0, int pageSize = int.MaxValue);

    Task<SmartCarousel> GetCarouselByIdAsync(int carouselId);

    Task InsertCarouselAsync(SmartCarousel carousel);

    Task UpdateCarouselAsync(SmartCarousel carousel);

    Task DeleteCarouselAsync(SmartCarousel carousel, bool deleteReletedData = true);

    #endregion

    #region Carousel products

    Task<IList<SmartCarouselProductMapping>> GetCarouselProductMappingsByCarouselIdAsync(int carouselId);

    Task<IList<Product>> GetProductsByCarouselIdAsync(int carouselId, int storeId = 0, int recordsToReturn = 0, bool activeOnly = true);

    Task<SmartCarouselProductMapping> GetCarouselProductMappingAsync(int carouselId, int productId);

    Task<SmartCarouselProductMapping> GetCarouselProductMappingByIdAsync(int carouselProductMappingId);

    Task InsertCarouselProductMappingAsync(SmartCarouselProductMapping carouselProductMapping);

    Task UpdateCarouselProductMappingAsync(SmartCarouselProductMapping carouselProductMapping);

    Task DeleteCarouselProductMappingAsync(SmartCarouselProductMapping carouselProductMapping);

    #endregion

    #region Carousel manufacturers

    Task<IList<SmartCarouselManufacturerMapping>> GetCarouselManufacturerMappingsByCarouselIdAsync(int carouselId);

    Task<IList<Manufacturer>> GetManufacturersByCarouselIdAsync(int carouselId, int storeId = 0, int recordsToReturn = 0, bool activeOnly = true);

    Task<SmartCarouselManufacturerMapping> GetCarouselManufacturerMappingAsync(int carouselId, int manufacturerId);

    Task<SmartCarouselManufacturerMapping> GetCarouselManufacturerMappingByIdAsync(int carouselManufacturerMappingId);

    Task InsertCarouselManufacturerMappingAsync(SmartCarouselManufacturerMapping carouselManufacturerMapping);

    Task UpdateCarouselManufacturerMappingAsync(SmartCarouselManufacturerMapping carouselManufacturerMapping);

    Task DeleteCarouselManufacturerMappingAsync(SmartCarouselManufacturerMapping carouselManufacturerMapping);

    #endregion

    #region Carousel categories

    Task<IList<SmartCarouselCategoryMapping>> GetCarouselCategoryMappingsByCarouselIdAsync(int carouselId);

    Task<IList<Category>> GetCategoriesByCarouselIdAsync(int carouselId, int storeId = 0, int recordsToReturn = 0, bool activeOnly = true);

    Task<SmartCarouselCategoryMapping> GetCarouselCategoryMappingAsync(int carouselId, int categoryId);

    Task<SmartCarouselCategoryMapping> GetCarouselCategoryMappingByIdAsync(int carouselCategoryMappingId);

    Task InsertCarouselCategoryMappingAsync(SmartCarouselCategoryMapping carouselCategoryMapping);

    Task UpdateCarouselCategoryMappingAsync(SmartCarouselCategoryMapping carouselCategoryMapping);

    Task DeleteCarouselCategoryMappingAsync(SmartCarouselCategoryMapping carouselCategoryMapping);

    #endregion

    #region Carousel vendors

    Task<IList<SmartCarouselVendorMapping>> GetCarouselVendorMappingsByCarouselIdAsync(int carouselId);

    Task<IList<Vendor>> GetVendorsByCarouselIdAsync(int carouselId, int recordsToReturn = 0, bool activeOnly = true);

    Task<SmartCarouselVendorMapping> GetCarouselVendorMappingAsync(int carouselId, int vendorId);

    Task<SmartCarouselVendorMapping> GetCarouselVendorMappingByIdAsync(int carouselVendorMappingId);

    Task InsertCarouselVendorMappingAsync(SmartCarouselVendorMapping carouselVendorMapping);

    Task UpdateCarouselVendorMappingAsync(SmartCarouselVendorMapping carouselVendorMapping);

    Task DeleteCarouselVendorMappingAsync(SmartCarouselVendorMapping carouselVendorMapping);

    #endregion

    #region Carousel pictures

    Task<IList<SmartCarouselPictureMapping>> GetCarouselPictureMappingsByCarouselIdAsync(int carouselId);

    Task<SmartCarouselPictureMapping> GetCarouselPictureMappingByIdAsync(int carouselPictureMappingId);

    Task InsertCarouselPictureMappingAsync(SmartCarouselPictureMapping carouselPictureMapping);

    Task UpdateCarouselPictureMappingAsync(SmartCarouselPictureMapping carouselPictureMapping);

    Task DeleteCarouselPictureMappingAsync(SmartCarouselPictureMapping carouselPictureMapping);

    #endregion

    #region Common

    Task<IList<Vendor>> GetVendorsByIdsAsync(int[] vendorIds);

    #endregion
}