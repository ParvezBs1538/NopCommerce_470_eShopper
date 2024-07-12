using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Security;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public class QuoteRequestWhitelistService : IQuoteRequestWhitelistService
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IPermissionService _permissionService;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Manufacturer> _manufacturerRepository;
    private readonly IRepository<ProductCategory> _productCategoryRepository;
    private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<QuoteRequestWhitelist> _quoteRequestWhitelistRepository;
    private readonly IRepository<Vendor> _vendorRepository;
    private readonly IWorkContext _workContext;
    private readonly QuoteCartSettings _quoteCartSettings;

    #endregion

    #region Ctor

    public QuoteRequestWhitelistService(
        ICustomerService customerService,
        IPermissionService permissionService,
        IRepository<Category> categoryRepository,
        IRepository<Manufacturer> manufacturerRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductManufacturer> productManufacturerRepository,
        IRepository<Product> productRepository,
        IRepository<QuoteRequestWhitelist> quoteRequestWhitelistRepository,
        IRepository<Vendor> vendorRepository,
        IWorkContext workContext,
        QuoteCartSettings quoteCartSettings)
    {
        _customerService = customerService;
        _permissionService = permissionService;
        _categoryRepository = categoryRepository;
        _manufacturerRepository = manufacturerRepository;
        _productCategoryRepository = productCategoryRepository;
        _productManufacturerRepository = productManufacturerRepository;
        _productRepository = productRepository;
        _quoteRequestWhitelistRepository = quoteRequestWhitelistRepository;
        _vendorRepository = vendorRepository;
        _workContext = workContext;
        _quoteCartSettings = quoteCartSettings;
    }

    #endregion

    #region Methods

    public virtual async Task<bool> CanQuoteAsync(int productId, Customer customer = null)
    {
        if (!_quoteCartSettings.EnableQuoteCart)
            return false;

        if (!await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.SendQuoteRequest))
            return false;

        if (!_quoteCartSettings.EnableWhitelist)
            return true;

        customer ??= await _workContext.GetCurrentCustomerAsync();

        if (_quoteCartSettings.WhitelistAllProducts)
            return true;

        if ((await _productRepository.GetByIdAsync(productId, cache => default))?.ProductType == ProductType.GroupedProduct)
            return false;

        var productQuery = from qp in _quoteRequestWhitelistRepository.Table
                           where qp.EntityName == nameof(Product) && qp.EntityId == productId
                           select qp.Id;

        var categoryQuery = from qp in _quoteRequestWhitelistRepository.Table
                            from pc in _productCategoryRepository.Table
                            where
                                pc.ProductId == productId &&
                                qp.EntityName == nameof(Category) &&
                                (_quoteCartSettings.WhitelistAllCategories || pc.CategoryId == qp.EntityId)
                            select qp.Id;

        var manufacturerQuery = from qp in _quoteRequestWhitelistRepository.Table
                                from pm in _productManufacturerRepository.Table
                                where
                                    pm.ProductId == productId &&
                                    qp.EntityName == nameof(Manufacturer) &&
                                    (_quoteCartSettings.WhitelistAllManufacturers || pm.ManufacturerId == qp.EntityId)
                                select qp.Id;

        var vendorQuery = from qp in _quoteRequestWhitelistRepository.Table
                          from pm in _productRepository.Table
                          where
                              pm.VendorId == qp.EntityId &&
                              qp.EntityName == nameof(Vendor) &&
                              (_quoteCartSettings.WhitelistAllVendors || (pm.VendorId != 0 && pm.VendorId == qp.EntityId))
                          select qp.Id;

        return await productQuery.Union(categoryQuery).Union(manufacturerQuery).Union(vendorQuery).AnyAsync();
    }

    public virtual async Task<IPagedList<QuoteRequestWhitelist>> GetWhitelistedEntityListAsync<T>(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _quoteRequestWhitelistRepository
            .GetAllPagedAsync(x => x.Where(entity => entity.EntityName == typeof(T).Name), pageIndex, pageSize);
    }

    public virtual async Task<IPagedList<Product>> GetWhitelistedProductsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = from qcw in _quoteRequestWhitelistRepository.Table
                    where qcw.EntityName == nameof(Product)
                    join p in _productRepository.Table on qcw.EntityId equals p.Id
                    select p;
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<IPagedList<Category>> GetWhitelistedCategoriesAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = from qcw in _quoteRequestWhitelistRepository.Table
                    where qcw.EntityName == nameof(Category)
                    join c in _categoryRepository.Table on qcw.EntityId equals c.Id
                    select c;
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<IPagedList<Manufacturer>> GetWhitelistedManufacturersAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = from qcw in _quoteRequestWhitelistRepository.Table
                    where qcw.EntityName == nameof(Manufacturer)
                    join m in _manufacturerRepository.Table on qcw.EntityId equals m.Id
                    select m;
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<IPagedList<Vendor>> GetWhitelistedVendorsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = from qcw in _quoteRequestWhitelistRepository.Table
                    where qcw.EntityName == nameof(Vendor)
                    join v in _vendorRepository.Table on qcw.EntityId equals v.Id
                    select v;
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task AddPermissionsAsync<T>(IList<int> entityIds) where T : BaseEntity
    {
        var excludeIds = (await GetWhitelistedEntityListAsync<T>()).Select(x => x.EntityId);

        var idsToAdd = entityIds.Except(excludeIds);

        if (!idsToAdd.Any())
            return;

        await _quoteRequestWhitelistRepository.InsertAsync(
            idsToAdd
                .Select(x => new QuoteRequestWhitelist
                {
                    EntityId = x,
                    EntityName = typeof(T).Name
                })
                .ToList());
    }

    public async Task RemovePermissionsAsync<T>(IList<int> ids) where T : BaseEntity
    {
        await _quoteRequestWhitelistRepository
            .DeleteAsync(x => x.EntityName == typeof(T).Name && ids.Contains(x.EntityId));
    }

    public async Task RemovePermissionAsync<T>(int id) where T : BaseEntity
    {
        await _quoteRequestWhitelistRepository
            .DeleteAsync(x => x.EntityName == typeof(T).Name && id == x.EntityId);
    }

    #endregion
}
