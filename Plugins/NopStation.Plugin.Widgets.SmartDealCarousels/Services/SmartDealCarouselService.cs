using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Data;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;
using NopStation.Plugin.Widgets.SmartDealCarousels.Services.Cache;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Services;

public class SmartDealCarouselService : ISmartDealCarouselService
{
    #region Fields

    private readonly IStoreMappingService _storeMappingService;
    private readonly IRepository<SmartDealCarousel> _carouselRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<SmartDealCarouselProductMapping> _carouselProductRepository;
    private readonly IRepository<DiscountManufacturerMapping> _discountManufacturerMappingRepository;
    private readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
    private readonly IRepository<ProductCategory> _productCategoryRepository;
    private readonly IRepository<DiscountCategoryMapping> _discountCategoryMappingRepository;
    private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
    private readonly IConditionService _conditionService;
    private readonly IWorkContext _workContext;
    private readonly IAclService _aclService;
    private readonly IScheduleService _scheduleService;
    private readonly IWidgetZoneService _widgetZoneService;
    private readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public SmartDealCarouselService(IStoreMappingService storeMappingService,
        IRepository<SmartDealCarousel> carouselRepository,
        IRepository<Product> productRepository,
        IRepository<SmartDealCarouselProductMapping> carouselProductRepository,
        IRepository<DiscountManufacturerMapping> discountManufacturerMappingRepository,
        IRepository<DiscountProductMapping> discountProductMappingRepository,
        IRepository<DiscountCategoryMapping> discountCategoryMappingRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductManufacturer> productManufacturerRepository,
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
        _discountManufacturerMappingRepository = discountManufacturerMappingRepository;
        _discountProductMappingRepository = discountProductMappingRepository;
        _productCategoryRepository = productCategoryRepository;
        _discountCategoryMappingRepository = discountCategoryMappingRepository;
        _productManufacturerRepository = productManufacturerRepository;
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

    public virtual async Task<IPagedList<SmartDealCarousel>> GetAllCarouselsAsync(string keywords = null, int storeId = 0, int productId = 0,
        bool overrideProduct = false, bool showHidden = false, bool? overridePublished = null, bool validScheduleOnly = false,
        int productSourceTypeId = 0, string widgetZone = null, int pageIndex = 0, int pageSize = int.MaxValue)
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
            query = query.Where(s => s.Name.Contains(keywords) || s.Title.Contains(keywords));

        //apply schedule mapping constraints
        if (validScheduleOnly)
            query = await _scheduleService.ApplyScheduleMappingAsync(query);

        query = query.OrderBy(c => c.DisplayOrder);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<SmartDealCarousel> GetCarouselByIdAsync(int carouselId)
    {
        if (carouselId == 0)
            return null;

        return await _carouselRepository.GetByIdAsync(carouselId, cache => default);
    }

    public virtual async Task InsertCarouselAsync(SmartDealCarousel carousel)
    {
        await _carouselRepository.InsertAsync(carousel);
    }

    public virtual async Task UpdateCarouselAsync(SmartDealCarousel carousel)
    {
        await _carouselRepository.UpdateAsync(carousel);
    }

    public virtual async Task DeleteCarouselAsync(SmartDealCarousel carousel, bool deleteReletedData = true)
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

    public virtual async Task<IList<SmartDealCarouselProductMapping>> GetCarouselProductMappingsByCarouselIdAsync(int carouselId)
    {
        var query = from cp in _carouselProductRepository.Table
                    join p in _productRepository.Table on cp.ProductId equals p.Id
                    where !p.Deleted && cp.CarouselId == carouselId
                    orderby cp.DisplayOrder
                    select cp;

        return await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.CarouselProductMappingsKey, carouselId),
            async () => await query.ToListAsync());
    }

    public virtual async Task<IPagedList<Product>> GetProductsWithAppliedDiscountAsync(Discount discount,
        bool showHidden = false, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        if (discount.DiscountType != DiscountType.AssignedToSkus &&
            discount.DiscountType != DiscountType.AssignedToManufacturers &&
            discount.DiscountType != DiscountType.AssignedToCategories)
            throw new InvalidProgramException(nameof(discount));

        var query = _productRepository.Table.Where(p => !p.Deleted && p.HasDiscountsApplied);

        switch (discount.DiscountType)
        {
            case DiscountType.AssignedToSkus:
                query = from p in query
                        join dpm in _discountProductMappingRepository.Table on p.Id equals dpm.EntityId
                        where dpm.DiscountId == discount.Id
                        select p;
                break;
            case DiscountType.AssignedToCategories:
                query = from p in query
                        join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId
                        join dcm in _discountCategoryMappingRepository.Table on p.Id equals dcm.EntityId
                        where dcm.DiscountId == discount.Id
                        select p;
                break;
            case DiscountType.AssignedToManufacturers:
            default:
                query = from p in query
                        join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId
                        join dmm in _discountManufacturerMappingRepository.Table on p.Id equals dmm.EntityId
                        where dmm.DiscountId == discount.Id
                        select p;
                break;
        }

        if (!showHidden)
        {
            query = query.Where(p => p.Published && p.VisibleIndividually &&
                        DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                        DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue));

            var customer = await _workContext.GetCurrentCustomerAsync();

            //apply ACL constraints
            query = await _aclService.ApplyAcl(query, customer);
            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);
        }

        query = query.OrderBy(product => product.DisplayOrder).ThenBy(product => product.Id);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<IList<Product>> GetProductsByCarouselIdAsync(int carouselId, int storeId = 0, int recordsToReturn = 0, bool activeOnly = true)
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

        return await query.ToListAsync();
    }

    public virtual async Task<SmartDealCarouselProductMapping> GetCarouselProductMappingAsync(int carouselId, int productId)
    {
        if (carouselId == 0 || productId == 0)
            return null;

        return await _carouselProductRepository.Table.FirstOrDefaultAsync(x => x.CarouselId == carouselId && x.ProductId == productId);
    }

    public virtual async Task<SmartDealCarouselProductMapping> GetCarouselProductMappingByIdAsync(int carouselProductMappingId)
    {
        if (carouselProductMappingId == 0)
            return null;

        return await _carouselProductRepository.GetByIdAsync(carouselProductMappingId, cache => default);
    }

    public virtual async Task InsertCarouselProductMappingAsync(SmartDealCarouselProductMapping carouselProductMapping)
    {
        await _carouselProductRepository.InsertAsync(carouselProductMapping);
    }

    public virtual async Task UpdateCarouselProductMappingAsync(SmartDealCarouselProductMapping carouselProductMapping)
    {
        await _carouselProductRepository.UpdateAsync(carouselProductMapping);
    }

    public virtual async Task DeleteCarouselProductMappingAsync(SmartDealCarouselProductMapping carouselProductMapping)
    {
        await _carouselProductRepository.DeleteAsync(carouselProductMapping);
    }

    #endregion

    #endregion
}
