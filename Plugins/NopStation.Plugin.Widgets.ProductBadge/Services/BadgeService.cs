using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Services;

public class BadgeService : IBadgeService
{
    #region Fields

    private readonly IStoreMappingService _storeMappingService;
    private readonly IRepository<Badge> _badgeRepository;
    private readonly IRepository<BadgeCategoryMapping> _badgeCategoryMappingRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<BadgeManufacturerMapping> _badgeManufacturerMappingRepository;
    private readonly IRepository<Manufacturer> _manufacturerRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<BadgeProductMapping> _badgeProductMappingRepository;
    private readonly IRepository<Vendor> _vendorRepository;
    private readonly IRepository<BadgeVendorMapping> _badgeVendorMappingRepository;
    private readonly IStaticCacheManager _cacheManager;
    private readonly ProductBadgeSettings _productBadgeSettings;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly ICustomerService _customerService;
    private readonly IAclService _aclService;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IRepository<Order> _orderRepository;

    #endregion Fields

    #region Ctor

    public BadgeService(IStoreMappingService storeMappingService,
        IRepository<Badge> badgeRepository,
        IRepository<BadgeCategoryMapping> badgeCategoryMappingRepository,
        IRepository<Category> categoryRepository,
        IRepository<BadgeManufacturerMapping> badgeManufacturerMappingRepository,
        IRepository<Manufacturer> manufacturerRepository,
        IRepository<Product> productRepository,
        IRepository<BadgeProductMapping> badgeProductMappingRepository,
        IRepository<Vendor> vendorRepository,
        IRepository<BadgeVendorMapping> badgeVendorMappingRepository,
        IStaticCacheManager cacheManager,
        ProductBadgeSettings productBadgeSettings,
        IStoreContext storeContext,
        IWorkContext workContext,
        ICustomerService customerService,
        IAclService aclService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IPriceCalculationService priceCalculationService,
        IRepository<OrderItem> orderItemRepository,
        IRepository<Order> orderRepository)
    {
        _storeMappingService = storeMappingService;
        _badgeRepository = badgeRepository;
        _badgeCategoryMappingRepository = badgeCategoryMappingRepository;
        _categoryRepository = categoryRepository;
        _badgeManufacturerMappingRepository = badgeManufacturerMappingRepository;
        _manufacturerRepository = manufacturerRepository;
        _productRepository = productRepository;
        _badgeProductMappingRepository = badgeProductMappingRepository;
        _vendorRepository = vendorRepository;
        _badgeVendorMappingRepository = badgeVendorMappingRepository;
        _cacheManager = cacheManager;
        _productBadgeSettings = productBadgeSettings;
        _storeContext = storeContext;
        _workContext = workContext;
        _customerService = customerService;
        _aclService = aclService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _priceCalculationService = priceCalculationService;
        _orderItemRepository = orderItemRepository;
        _orderRepository = orderRepository;
    }

    #endregion

    #region Utilities

    protected async Task<bool> IsBestSellingProductAsync(Badge badge, Product product)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeBestSellingProductKey, badge, product);

        return await _cacheManager.GetAsync(cacheKey, () =>
        {
            var osIds = badge.BestSellOrderStatusIds.ToIntList();
            var psIds = badge.BestSellPaymentStatusIds.ToIntList();
            var ssIds = badge.BestSellShippingStatusIds.ToIntList();

            var createdFromUtc = DateTime.UtcNow.AddDays(-badge.BestSellSoldInDays);
            var storeId = badge.BestSellStoreWise ? _storeContext.GetCurrentStore().Id : 0;

            var reportLine = (from orderItem in _orderItemRepository.Table
                              join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                              where orderItem.ProductId == product.Id &&
                                 (storeId == 0 || storeId == o.StoreId) &&
                                 createdFromUtc <= o.CreatedOnUtc &&
                                 (!osIds.Any() || osIds.Contains(o.OrderStatusId)) &&
                                 (!psIds.Any() || psIds.Contains(o.PaymentStatusId)) &&
                                 (!ssIds.Any() || ssIds.Contains(o.ShippingStatusId)) &&
                                 !o.Deleted
                              group orderItem by orderItem.ProductId into g
                              select new BestsellersReportLine
                              {
                                  ProductId = g.Key,
                                  TotalAmount = g.Sum(x => x.PriceExclTax),
                                  TotalQuantity = g.Sum(x => x.Quantity)
                              }).FirstOrDefault();

            return reportLine != null && reportLine.TotalQuantity >= badge.BestSellMinimumQuantitySold &&
                reportLine.TotalAmount >= badge.BestSellMinimumAmountSold;
        });
    }

    #endregion

    #region Methods

    #region Badges

    public virtual async Task InsertBadgeAsync(Badge badge)
    {
        await _badgeRepository.InsertAsync(badge);
    }

    public virtual async Task UpdateBadgeAsync(Badge badge)
    {
        await _badgeRepository.UpdateAsync(badge);
    }

    public virtual async Task DeleteBadgeAsync(Badge badge)
    {
        await _badgeRepository.DeleteAsync(badge);
    }

    public virtual async Task<Badge> GetBadgeByIdAsync(int badgeId)
    {
        return await _badgeRepository.GetByIdAsync(badgeId, cache => default);
    }

    public virtual async Task<IPagedList<Badge>> GetAllBadgesAsync(string keywords = null, int storeId = 0, bool showHidden = false,
        bool? overridePublished = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _badgeRepository.Table.Where(b => !b.Deleted);

        if (!showHidden)
            query = query.Where(b => b.Active && DateTime.UtcNow >= (b.FromDateTimeUtc ?? DateTime.MinValue) && DateTime.UtcNow <= (b.ToDateTimeUtc ?? DateTime.MaxValue));
        else if (overridePublished.HasValue)
            query = query.Where(b => b.Active == overridePublished.Value);

        if (!showHidden)
        {
            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            query = await _aclService.ApplyAcl(query, customer);
        }

        if (!string.IsNullOrWhiteSpace(keywords))
            query = query.Where(b => b.Name.Contains(keywords) || b.Text.Contains(keywords));

        //apply store mapping constraints
        query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        query = query.OrderByDescending(x => x.Id);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<IList<Badge>> GetActiveBadgesAsync(int storeId = 0)
    {
        var crs = await _customerService.GetCustomerRolesAsync(await _workContext.GetCurrentCustomerAsync());
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.ActiveBadgesKey, storeId, crs, DateTime.UtcNow.Date);

        if (!_productBadgeSettings.CacheActiveBadges)
            cacheKey.CacheTime = 0;

        var popups = await _badgeRepository.GetAllAsync(async query =>
        {
            if (!_productBadgeSettings.CacheActiveBadges)
                query = query.Where(b => b.Active &&
                    DateTime.UtcNow >= (b.FromDateTimeUtc ?? DateTime.MinValue) &&
                    DateTime.UtcNow <= (b.ToDateTimeUtc ?? DateTime.MaxValue));
            else
                query = query.Where(b => b.Active &&
                    DateTime.UtcNow.Date >= (b.FromDateTimeUtc ?? DateTime.MinValue).Date &&
                    DateTime.UtcNow.Date <= (b.ToDateTimeUtc ?? DateTime.MaxValue).Date);

            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            query = await _aclService.ApplyAcl(query, customer);

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            return query.OrderByDescending(x => x.Id);
        }, cache => cacheKey, false);

        //validate time when cache enabled
        if (_productBadgeSettings.CacheActiveBadges)
            popups = popups.Where(b =>
                DateTime.UtcNow >= (b.FromDateTimeUtc ?? DateTime.MinValue) &&
                DateTime.UtcNow <= (b.ToDateTimeUtc ?? DateTime.MaxValue)).ToList();

        return popups;
    }

    public virtual async Task<IList<Badge>> GetProductBadgesAsync(Product product)
    {
        return await (await GetActiveBadgesAsync(_storeContext.GetCurrentStore().Id))
            .WhereAwait(async b => await ValidateBadgeAsync(b, product))
            .ToListAsync();
    }

    public virtual async Task<bool> ValidateBadgeAsync(Badge badge, Product product)
    {
        if (badge == null)
            throw new ArgumentNullException(nameof(badge));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        switch (badge.BadgeType)
        {
            case BadgeType.FreeShippingProducts:
                return product.IsShipEnabled && product.IsFreeShipping;
            case BadgeType.NewProducts:
                return product.MarkAsNew &&
                    DateTime.UtcNow >= (product.MarkAsNewStartDateTimeUtc ?? DateTime.MinValue) &&
                    DateTime.UtcNow <= (product.MarkAsNewEndDateTimeUtc ?? DateTime.MaxValue);
            case BadgeType.FreeProducts:
                return (await _priceCalculationService.GetFinalPriceAsync(product, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync())).finalPrice == 0;
            case BadgeType.DiscountedProducts:
                return (await _priceCalculationService.GetFinalPriceAsync(product, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync())).appliedDiscountAmount > 0;
            case BadgeType.BestSellingProducts:
                return await IsBestSellingProductAsync(badge, product);
            case BadgeType.CustomProducts:
            default:
                break;
        }

        switch (badge.CatalogType)
        {
            case CatalogType.Products:
                return (await GetProductsByBadgeIdAsync(badge.Id)).Any(p => p.Id == product.Id);
            case CatalogType.Categories:
                {
                    var cids = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).Select(pcs => pcs.CategoryId);
                    if (!cids.Any())
                        return false;

                    foreach (var c in await GetCategoriesByBadgeIdAsync(badge.Id))
                        if (cids.Contains(c.Id))
                            return true;

                    return false;
                }
            case CatalogType.Manufacturers:
                {
                    var mids = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).Select(pcs => pcs.ManufacturerId);
                    if (!mids.Any())
                        return false;

                    foreach (var m in await GetManufacturersByBadgeIdAsync(badge.Id))
                        if (mids.Contains(m.Id))
                            return true;

                    return false;
                }
            case CatalogType.Vendors:
                if (product.VendorId == 0)
                    return false;

                return (await GetVendorsByBadgeIdAsync(badge.Id)).Any(b => b.Id == product.VendorId);
            default:
                throw new NotImplementedException();
        }
    }

    #endregion

    #region Badge product mappings

    public virtual async Task<BadgeProductMapping> GetBadgeProductMappingByIdAsync(int badgeProductMappingId)
    {
        return await _badgeProductMappingRepository.GetByIdAsync(badgeProductMappingId);
    }

    public virtual async Task InsertBadgeProductMappingAsync(BadgeProductMapping badgeProductMapping)
    {
        await _badgeProductMappingRepository.InsertAsync(badgeProductMapping);
    }

    public virtual async Task UpdateBadgeProductMappingAsync(BadgeProductMapping badgeProductMapping)
    {
        await _badgeProductMappingRepository.UpdateAsync(badgeProductMapping);
    }

    public virtual async Task DeleteBadgeProductMappingAsync(BadgeProductMapping badgeProductMapping)
    {
        await _badgeProductMappingRepository.DeleteAsync(badgeProductMapping);
    }

    public virtual async Task<IList<BadgeProductMapping>> GetBadgeProductMappingsAsync(int badgeId)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeProductMappingsKey, badgeId);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from bpm in _badgeProductMappingRepository.Table
                        join p in _productRepository.Table on bpm.ProductId equals p.Id
                        where !p.Deleted && bpm.BadgeId == badgeId
                        select bpm;

            return await query.ToListAsync();
        });
    }

    public virtual async Task<IList<Product>> GetProductsByBadgeIdAsync(int badgeId)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeProductsKey, badgeId);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from bpm in _badgeProductMappingRepository.Table
                        join p in _productRepository.Table on bpm.ProductId equals p.Id
                        where !p.Deleted && bpm.BadgeId == badgeId
                        select p;

            return await query.ToListAsync();
        });
    }

    #endregion

    #region Badge category mappings

    public virtual async Task<BadgeCategoryMapping> GetBadgeCategoryMappingByIdAsync(int badgeCategoryMappingId)
    {
        return await _badgeCategoryMappingRepository.GetByIdAsync(badgeCategoryMappingId);
    }

    public virtual async Task InsertBadgeCategoryMappingAsync(BadgeCategoryMapping badgeCategoryMapping)
    {
        await _badgeCategoryMappingRepository.InsertAsync(badgeCategoryMapping);
    }

    public virtual async Task UpdateBadgeCategoryMappingAsync(BadgeCategoryMapping badgeCategoryMapping)
    {
        await _badgeCategoryMappingRepository.UpdateAsync(badgeCategoryMapping);
    }

    public virtual async Task DeleteBadgeCategoryMappingAsync(BadgeCategoryMapping badgeCategoryMapping)
    {
        await _badgeCategoryMappingRepository.DeleteAsync(badgeCategoryMapping);
    }

    public virtual async Task<IList<BadgeCategoryMapping>> GetBadgeCategoryMappingsAsync(int badgeId, bool active = true)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeCategoryMappingsKey, badgeId, active);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from bcm in _badgeCategoryMappingRepository.Table
                        join c in _categoryRepository.Table on bcm.CategoryId equals c.Id
                        where !c.Deleted && (!active || c.Published) && bcm.BadgeId == badgeId
                        select bcm;

            return await query.ToListAsync();
        });
    }

    public virtual async Task<IList<Category>> GetCategoriesByBadgeIdAsync(int badgeId, bool active = true)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeCategoriesKey, badgeId, active);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from bcm in _badgeCategoryMappingRepository.Table
                        join c in _categoryRepository.Table on bcm.CategoryId equals c.Id
                        where !c.Deleted && (!active || c.Published) && bcm.BadgeId == badgeId
                        select c;

            return await query.ToListAsync();
        });
    }

    #endregion

    #region Badge manufacturer mappings

    public virtual async Task<BadgeManufacturerMapping> GetBadgeManufacturerMappingByIdAsync(int badgeManufacturerMappingId)
    {
        return await _badgeManufacturerMappingRepository.GetByIdAsync(badgeManufacturerMappingId);
    }

    public virtual async Task InsertBadgeManufacturerMappingAsync(BadgeManufacturerMapping badgeManufacturerMapping)
    {
        await _badgeManufacturerMappingRepository.InsertAsync(badgeManufacturerMapping);
    }

    public virtual async Task UpdateBadgeManufacturerMappingAsync(BadgeManufacturerMapping badgeManufacturerMapping)
    {
        await _badgeManufacturerMappingRepository.UpdateAsync(badgeManufacturerMapping);
    }

    public virtual async Task DeleteBadgeManufacturerMappingAsync(BadgeManufacturerMapping badgeManufacturerMapping)
    {
        await _badgeManufacturerMappingRepository.DeleteAsync(badgeManufacturerMapping);
    }

    public virtual async Task<IList<BadgeManufacturerMapping>> GetBadgeManufacturerMappingsAsync(int badgeId, bool active = true)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeManufacturerMappingsKey, badgeId, active);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from bmm in _badgeManufacturerMappingRepository.Table
                        join m in _manufacturerRepository.Table on bmm.ManufacturerId equals m.Id
                        where !m.Deleted && (!active || m.Published) && bmm.BadgeId == badgeId
                        select bmm;

            return await query.ToListAsync();
        });
    }

    public virtual async Task<IList<Manufacturer>> GetManufacturersByBadgeIdAsync(int badgeId, bool active = true)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeManufacturersKey, badgeId, active);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from bmm in _badgeManufacturerMappingRepository.Table
                        join m in _manufacturerRepository.Table on bmm.ManufacturerId equals m.Id
                        where !m.Deleted && (!active || m.Published) && bmm.BadgeId == badgeId
                        select m;

            return await query.ToListAsync();
        });
    }

    #endregion

    #region Badge vendor mappings

    public virtual async Task<BadgeVendorMapping> GetBadgeVendorMappingByIdAsync(int badgeVendorMappingId)
    {
        return await _badgeVendorMappingRepository.GetByIdAsync(badgeVendorMappingId);
    }

    public virtual async Task InsertBadgeVendorMappingAsync(BadgeVendorMapping badgeVendorMapping)
    {
        await _badgeVendorMappingRepository.InsertAsync(badgeVendorMapping);
    }

    public virtual async Task UpdateBadgeVendorMappingAsync(BadgeVendorMapping badgeVendorMapping)
    {
        await _badgeVendorMappingRepository.UpdateAsync(badgeVendorMapping);
    }

    public virtual async Task DeleteBadgeVendorMappingAsync(BadgeVendorMapping badgeVendorMapping)
    {
        await _badgeVendorMappingRepository.DeleteAsync(badgeVendorMapping);
    }

    public virtual async Task<IList<BadgeVendorMapping>> GetBadgeVendorMappingsAsync(int badgeId, bool active = true)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeVendorMappingsKey, badgeId, active);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from bvm in _badgeVendorMappingRepository.Table
                        join v in _vendorRepository.Table on bvm.VendorId equals v.Id
                        where !v.Deleted && (!active || v.Active) && bvm.BadgeId == badgeId
                        select bvm;

            return await query.ToListAsync();
        });
    }

    public virtual async Task<IList<Vendor>> GetVendorsByBadgeIdAsync(int badgeId, bool active = true)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeVendorsKey, badgeId, active);

        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from bvm in _badgeVendorMappingRepository.Table
                        join v in _vendorRepository.Table on bvm.VendorId equals v.Id
                        where !v.Deleted && (!active || v.Active) && bvm.BadgeId == badgeId
                        select v;

            return await query.ToListAsync();
        });
    }

    #endregion

    #endregion
}