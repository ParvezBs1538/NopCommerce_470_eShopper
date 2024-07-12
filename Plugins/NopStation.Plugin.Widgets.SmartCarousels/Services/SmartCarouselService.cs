using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;
using NopStation.Plugin.Widgets.SmartCarousels.Services.Cache;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services;

public class SmartCarouselService : ISmartCarouselService
{
    #region Fields

    private readonly IStoreMappingService _storeMappingService;
    private readonly IRepository<SmartCarousel> _carouselRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<SmartCarouselProductMapping> _carouselProductRepository;
    private readonly IRepository<Manufacturer> _manufacturerRepository;
    private readonly IRepository<SmartCarouselManufacturerMapping> _carouselManufacturerRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<SmartCarouselCategoryMapping> _carouselCategoryRepository;
    private readonly IRepository<SmartCarouselVendorMapping> _carouselVendorRepository;
    private readonly IRepository<SmartCarouselPictureMapping> _carouselPictureMappingRepository;
    private readonly IRepository<Vendor> _vendorRepository;
    private readonly IConditionService _conditionService;
    private readonly IWorkContext _workContext;
    private readonly IAclService _aclService;
    private readonly IScheduleService _scheduleService;
    private readonly IWidgetZoneService _widgetZoneService;
    private readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public SmartCarouselService(IStoreMappingService storeMappingService,
        IRepository<SmartCarousel> carouselRepository,
        IRepository<Product> productRepository,
        IRepository<SmartCarouselProductMapping> carouselProductRepository,
        IRepository<Manufacturer> manufacturerRepository,
        IRepository<SmartCarouselManufacturerMapping> carouselManufacturerRepository,
        IRepository<Category> categoryRepository,
        IRepository<SmartCarouselCategoryMapping> carouselCategoryRepository,
        IRepository<SmartCarouselVendorMapping> carouselVendorRepository,
        IRepository<SmartCarouselPictureMapping> carouselPictureMappingRepository,
        IRepository<Vendor> vendorRepository,
        IConditionService conditionService,
        IWorkContext workContext,
        IAclService aclService,
        IScheduleService scheduleService,
        IWidgetZoneService widgetZoneService,
        IStaticCacheManager staticCacheManager)
    {
        _storeMappingService = storeMappingService;
        _carouselRepository = carouselRepository;
        _productRepository = productRepository;
        _carouselProductRepository = carouselProductRepository;
        _manufacturerRepository = manufacturerRepository;
        _carouselManufacturerRepository = carouselManufacturerRepository;
        _categoryRepository = categoryRepository;
        _carouselCategoryRepository = carouselCategoryRepository;
        _carouselVendorRepository = carouselVendorRepository;
        _carouselPictureMappingRepository = carouselPictureMappingRepository;
        _vendorRepository = vendorRepository;
        _conditionService = conditionService;
        _workContext = workContext;
        _aclService = aclService;
        _scheduleService = scheduleService;
        _widgetZoneService = widgetZoneService;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    #region Carousels

    public virtual async Task<IPagedList<SmartCarousel>> GetAllCarouselsAsync(string keywords = null, int storeId = 0,
        int productId = 0, bool overrideProduct = false, bool showHidden = false, bool? overridePublished = null,
        bool validScheduleOnly = false, int carouselTypeId = 0, int productSourceTypeId = 0, string widgetZone = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _carouselRepository.Table.Where(c => !c.Deleted);

        if (!showHidden)
            query = query.Where(s => s.Active);
        else if (overridePublished.HasValue)
            query = query.Where(s => s.Active == overridePublished.Value);

        //apply store mapping constraints
        query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        //apply product condition mapping constraints
        if (overrideProduct)
            query = await _conditionService.ApplyProductConditionMappingAsync(query, productId);

        if (carouselTypeId > 0)
            query = query.Where(c => c.CarouselTypeId == carouselTypeId);

        if (productSourceTypeId > 0)
            query = query.Where(s => s.ProductSourceTypeId == productSourceTypeId);

        //apply widget zone mapping constraints
        query = await _widgetZoneService.ApplyWidgetZoneMappingAsync(query, widgetZone);

        if (!showHidden)
        {
            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            query = await _aclService.ApplyAcl(query, customer);

            //apply customer condition mapping constraints
            query = await _conditionService.ApplyCustomerConditionMappingAsync(query, customer.Id);
        }

        if (!string.IsNullOrWhiteSpace(keywords))
            query = query.Where(c => c.Name.Contains(keywords) || c.Title.Contains(keywords));

        //apply schedule mapping constraints
        if (validScheduleOnly)
            query = await _scheduleService.ApplyScheduleMappingAsync(query);

        query = query.OrderBy(c => c.DisplayOrder);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async virtual Task<SmartCarousel> GetCarouselByIdAsync(int carouselId)
    {
        if (carouselId == 0)
            return null;

        return await _carouselRepository.GetByIdAsync(carouselId, cache => default);
    }

    public async virtual Task InsertCarouselAsync(SmartCarousel carousel)
    {
        await _carouselRepository.InsertAsync(carousel);
    }

    public async virtual Task UpdateCarouselAsync(SmartCarousel carousel)
    {
        await _carouselRepository.UpdateAsync(carousel);
    }

    public async virtual Task DeleteCarouselAsync(SmartCarousel carousel, bool deleteReletedData = true)
    {
        if (deleteReletedData)
        {
            var widgetZoneMappings = await _widgetZoneService.GetEntityWidgetZoneMappingsAsync(carousel);
            await _widgetZoneService.DeleteWidgetZoneMappingsAsync(widgetZoneMappings);

            var customerConditions = await _conditionService.GetEntityCustomerConditionsAsync(carousel);
            await _conditionService.DeleteCustomerConditionMappingsAsync(customerConditions);

            var productConditions = await _conditionService.GetEntityProductConditionsAsync(carousel);
            await _conditionService.DeleteProductConditionMappingsAsync(productConditions);
        }

        await _carouselRepository.DeleteAsync(carousel);
    }

    #endregion

    #region Carousel products

    public async virtual Task<IList<SmartCarouselProductMapping>> GetCarouselProductMappingsByCarouselIdAsync(int carouselId)
    {
        var query = from cp in _carouselProductRepository.Table
                    join p in _productRepository.Table on cp.ProductId equals p.Id
                    where !p.Deleted && cp.CarouselId == carouselId
                    orderby cp.DisplayOrder
                    select cp;

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselProductMappingsKey, carouselId),
            async () => await query.ToListAsync());
    }

    public async virtual Task<IList<Product>> GetProductsByCarouselIdAsync(int carouselId, int storeId = 0, int recordsToReturn = 0, bool activeOnly = true)
    {
        var query = from p in _productRepository.Table
                    join cp in _carouselProductRepository.Table on p.Id equals cp.ProductId
                    where !p.Deleted && cp.CarouselId == carouselId && (!activeOnly || (p.Published && p.VisibleIndividually &&
                        DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                        DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)))
                    orderby cp.DisplayOrder
                    select p;

        var customer = await _workContext.GetCurrentCustomerAsync();

        if (activeOnly)
        {
            //apply ACL constraints
            query = await _aclService.ApplyAcl(query, customer);
            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);
        }

        if (recordsToReturn > 0)
            query = query.Take(recordsToReturn);

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselProductsKey, carouselId, customer, storeId, activeOnly, recordsToReturn),
            async () => await query.ToListAsync());
    }

    public async virtual Task<SmartCarouselProductMapping> GetCarouselProductMappingAsync(int carouselId, int productId)
    {
        if (carouselId == 0 || productId == 0)
            return null;

        return await _carouselProductRepository.Table.FirstOrDefaultAsync(x => x.CarouselId == carouselId && x.ProductId == productId);
    }

    public async virtual Task<SmartCarouselProductMapping> GetCarouselProductMappingByIdAsync(int carouselProductMappingId)
    {
        if (carouselProductMappingId == 0)
            return null;

        return await _carouselProductRepository.GetByIdAsync(carouselProductMappingId, cache => default);
    }

    public async virtual Task InsertCarouselProductMappingAsync(SmartCarouselProductMapping carouselProductMapping)
    {
        await _carouselProductRepository.InsertAsync(carouselProductMapping);
    }

    public async virtual Task UpdateCarouselProductMappingAsync(SmartCarouselProductMapping carouselProductMapping)
    {
        await _carouselProductRepository.UpdateAsync(carouselProductMapping);
    }

    public async virtual Task DeleteCarouselProductMappingAsync(SmartCarouselProductMapping carouselProductMapping)
    {
        await _carouselProductRepository.DeleteAsync(carouselProductMapping);
    }

    #endregion

    #region Carousel manufacturers

    public async virtual Task<IList<SmartCarouselManufacturerMapping>> GetCarouselManufacturerMappingsByCarouselIdAsync(int carouselId)
    {
        var query = from cm in _carouselManufacturerRepository.Table
                    join m in _manufacturerRepository.Table on cm.ManufacturerId equals m.Id
                    where !m.Deleted && cm.CarouselId == carouselId
                    orderby cm.DisplayOrder
                    select cm;

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselManufacturerMappingsKey, carouselId),
            async () => await query.ToListAsync());
    }

    public async virtual Task<IList<Manufacturer>> GetManufacturersByCarouselIdAsync(int carouselId, int storeId = 0, int recordsToReturn = 0, bool activeOnly = true)
    {
        var query = from m in _manufacturerRepository.Table
                    join cm in _carouselManufacturerRepository.Table on m.Id equals cm.ManufacturerId
                    where !m.Deleted && cm.CarouselId == carouselId && (!activeOnly || m.Published)
                    orderby cm.DisplayOrder
                    select m;

        //apply ACL constraints
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (activeOnly)
        {
            query = await _aclService.ApplyAcl(query, customer);

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);
        }

        if (recordsToReturn > 0)
            query = query.Take(recordsToReturn);

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselManufacturersKey, carouselId, customer, storeId, activeOnly, recordsToReturn),
            async () => await query.ToListAsync());
    }

    public async virtual Task<SmartCarouselManufacturerMapping> GetCarouselManufacturerMappingAsync(int carouselId, int manufacturerId)
    {
        if (carouselId == 0 || manufacturerId == 0)
            return null;

        return await _carouselManufacturerRepository.Table.FirstOrDefaultAsync(x => x.CarouselId == carouselId && x.ManufacturerId == manufacturerId);
    }

    public async virtual Task<SmartCarouselManufacturerMapping> GetCarouselManufacturerMappingByIdAsync(int carouselManufacturerMappingId)
    {
        if (carouselManufacturerMappingId == 0)
            return null;

        return await _carouselManufacturerRepository.GetByIdAsync(carouselManufacturerMappingId, cache => default);
    }

    public async virtual Task InsertCarouselManufacturerMappingAsync(SmartCarouselManufacturerMapping carouselManufacturerMapping)
    {
        await _carouselManufacturerRepository.InsertAsync(carouselManufacturerMapping);
    }

    public async virtual Task UpdateCarouselManufacturerMappingAsync(SmartCarouselManufacturerMapping carouselManufacturerMapping)
    {
        await _carouselManufacturerRepository.UpdateAsync(carouselManufacturerMapping);
    }

    public async virtual Task DeleteCarouselManufacturerMappingAsync(SmartCarouselManufacturerMapping carouselManufacturerMapping)
    {
        await _carouselManufacturerRepository.DeleteAsync(carouselManufacturerMapping);
    }

    #endregion

    #region Carousel categories

    public async virtual Task<IList<SmartCarouselCategoryMapping>> GetCarouselCategoryMappingsByCarouselIdAsync(int carouselId)
    {
        var query = from cc in _carouselCategoryRepository.Table
                    join c in _categoryRepository.Table on cc.CategoryId equals c.Id
                    where !c.Deleted && cc.CarouselId == carouselId
                    orderby cc.DisplayOrder
                    select cc;

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselCategoryMappingsKey, carouselId),
            async () => await query.ToListAsync());
    }

    public async virtual Task<IList<Category>> GetCategoriesByCarouselIdAsync(int carouselId, int storeId = 0, int recordsToReturn = 0, bool activeOnly = true)
    {
        var query = from c in _categoryRepository.Table
                    join cc in _carouselCategoryRepository.Table on c.Id equals cc.CategoryId
                    where !c.Deleted && cc.CarouselId == carouselId && (!activeOnly || c.Published)
                    orderby cc.DisplayOrder
                    select c;

        //apply ACL constraints
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (activeOnly)
        {
            query = await _aclService.ApplyAcl(query, customer);

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);
        }

        if (recordsToReturn > 0)
            query = query.Take(recordsToReturn);

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselCategoriesKey, carouselId, customer, storeId, activeOnly, recordsToReturn),
            async () => await query.ToListAsync());
    }

    public async virtual Task<SmartCarouselCategoryMapping> GetCarouselCategoryMappingAsync(int carouselId, int categoryId)
    {
        if (carouselId == 0 || categoryId == 0)
            return null;

        return await _carouselCategoryRepository.Table.FirstOrDefaultAsync(x => x.CarouselId == carouselId && x.CategoryId == categoryId);
    }

    public async virtual Task<SmartCarouselCategoryMapping> GetCarouselCategoryMappingByIdAsync(int carouselCategoryMappingId)
    {
        if (carouselCategoryMappingId == 0)
            return null;

        return await _carouselCategoryRepository.GetByIdAsync(carouselCategoryMappingId, cache => default);
    }

    public async virtual Task InsertCarouselCategoryMappingAsync(SmartCarouselCategoryMapping carouselCategoryMapping)
    {
        await _carouselCategoryRepository.InsertAsync(carouselCategoryMapping);
    }

    public async virtual Task UpdateCarouselCategoryMappingAsync(SmartCarouselCategoryMapping carouselCategoryMapping)
    {
        await _carouselCategoryRepository.UpdateAsync(carouselCategoryMapping);
    }

    public async virtual Task DeleteCarouselCategoryMappingAsync(SmartCarouselCategoryMapping carouselCategoryMapping)
    {
        await _carouselCategoryRepository.DeleteAsync(carouselCategoryMapping);
    }

    #endregion

    #region Carousel vendors

    public async virtual Task<IList<SmartCarouselVendorMapping>> GetCarouselVendorMappingsByCarouselIdAsync(int carouselId)
    {
        var query = from cv in _carouselVendorRepository.Table
                    join v in _vendorRepository.Table on cv.VendorId equals v.Id
                    where !v.Deleted && cv.CarouselId == carouselId
                    orderby cv.DisplayOrder
                    select cv;

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselVendorMappingsKey, carouselId),
            async () => await query.ToListAsync());
    }

    public async virtual Task<IList<Vendor>> GetVendorsByCarouselIdAsync(int carouselId, int recordsToReturn = 0, bool activeOnly = true)
    {
        var query = from v in _vendorRepository.Table
                    join cv in _carouselVendorRepository.Table on v.Id equals cv.VendorId
                    where !v.Deleted && cv.CarouselId == carouselId && (!activeOnly || v.Active)
                    orderby cv.DisplayOrder
                    select v;

        if (recordsToReturn > 0)
            query = query.Take(recordsToReturn);

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselVendorsKey, carouselId, activeOnly, recordsToReturn),
            async () => await query.ToListAsync());
    }

    public async virtual Task<SmartCarouselVendorMapping> GetCarouselVendorMappingAsync(int carouselId, int vendorId)
    {
        if (carouselId == 0 || vendorId == 0)
            return null;

        return await _carouselVendorRepository.Table.FirstOrDefaultAsync(x => x.CarouselId == carouselId && x.VendorId == vendorId);
    }

    public async virtual Task<SmartCarouselVendorMapping> GetCarouselVendorMappingByIdAsync(int carouselVendorMappingId)
    {
        if (carouselVendorMappingId == 0)
            return null;

        return await _carouselVendorRepository.GetByIdAsync(carouselVendorMappingId, cache => default);
    }

    public async virtual Task InsertCarouselVendorMappingAsync(SmartCarouselVendorMapping carouselVendorMapping)
    {
        await _carouselVendorRepository.InsertAsync(carouselVendorMapping);
    }

    public async virtual Task UpdateCarouselVendorMappingAsync(SmartCarouselVendorMapping carouselVendorMapping)
    {
        await _carouselVendorRepository.UpdateAsync(carouselVendorMapping);
    }

    public async virtual Task DeleteCarouselVendorMappingAsync(SmartCarouselVendorMapping carouselVendorMapping)
    {
        await _carouselVendorRepository.DeleteAsync(carouselVendorMapping);
    }

    #endregion

    #region Carousel pictures

    public async virtual Task<IList<SmartCarouselPictureMapping>> GetCarouselPictureMappingsByCarouselIdAsync(int carouselId)
    {
        var query = from cp in _carouselPictureMappingRepository.Table
                    where cp.CarouselId == carouselId
                    orderby cp.DisplayOrder
                    select cp;

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselPictureMappingsKey, carouselId),
            async () => await query.ToListAsync());
    }

    public async virtual Task<SmartCarouselPictureMapping> GetCarouselPictureMappingByIdAsync(int carouselPictureMappingId)
    {
        if (carouselPictureMappingId == 0)
            return null;

        return await _carouselPictureMappingRepository.GetByIdAsync(carouselPictureMappingId, cache => default);
    }

    public async virtual Task InsertCarouselPictureMappingAsync(SmartCarouselPictureMapping carouselPictureMapping)
    {
        await _carouselPictureMappingRepository.InsertAsync(carouselPictureMapping);
    }

    public async virtual Task UpdateCarouselPictureMappingAsync(SmartCarouselPictureMapping carouselPictureMapping)
    {
        await _carouselPictureMappingRepository.UpdateAsync(carouselPictureMapping);
    }

    public async virtual Task DeleteCarouselPictureMappingAsync(SmartCarouselPictureMapping carouselPictureMapping)
    {
        await _carouselPictureMappingRepository.DeleteAsync(carouselPictureMapping);
    }

    #endregion

    #region Common

    public virtual async Task<IList<Vendor>> GetVendorsByIdsAsync(int[] vendorIds)
    {
        return await _vendorRepository.GetByIdsAsync(vendorIds, includeDeleted: false);
    }

    #endregion

    #endregion
}
