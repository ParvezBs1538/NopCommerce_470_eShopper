using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.PrevNextProduct.Domains;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Services;

public class PrevNextProductService : IPrevNextProductService
{
    #region Fields

    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductCategory> _productCategoryRepository;
    private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
    private readonly PrevNextProductSettings _prevNextProductSettings;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IRepository<Vendor> _vendorRepository;
    private readonly IWorkContext _workContext;
    private readonly IAclService _aclService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public PrevNextProductService(IRepository<Product> productRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductManufacturer> productManufacturerRepository,
        PrevNextProductSettings prevNextProductSettings,
        IProductService productService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IRepository<Vendor> vendorRepository,
        IWorkContext workContext,
        IAclService aclService,
        IStoreMappingService storeMappingService,
        IStoreContext storeContext)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _productManufacturerRepository = productManufacturerRepository;
        _prevNextProductSettings = prevNextProductSettings;
        _productService = productService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _vendorRepository = vendorRepository;
        _workContext = workContext;
        _aclService = aclService;
        _storeMappingService = storeMappingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Utilities

    protected async Task<IQueryable<Product>> GetProductsQueryAsync(Product product)
    {
        if (_prevNextProductSettings.NavigateBasedOnId == (int)NavigationType.Category)
        {
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
            if (!productCategories.Any())
                return null;

            var categoryId = productCategories[0].CategoryId;

            var productsQuery = from p in _productRepository.Table
                                where p.Published && !p.Deleted && p.VisibleIndividually &&
                                     DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                                     DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                                select p;

            var productCategoryQuery =
                from pc in _productCategoryRepository.Table
                where pc.CategoryId == categoryId
                group pc by pc.ProductId into pc
                select new
                {
                    ProductId = pc.Key,
                    DisplayOrder = pc.First().DisplayOrder
                };

            productsQuery =
                from p in productsQuery
                join pc in productCategoryQuery on p.Id equals pc.ProductId
                orderby pc.DisplayOrder, p.Name
                select p;

            return productsQuery;
        }
        else if (_prevNextProductSettings.NavigateBasedOnId == (int)NavigationType.Manufacturer)
        {
            var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
            if (!productManufacturers.Any())
                return null;

            var manufacturerId = productManufacturers[0].ManufacturerId;

            var productsQuery = from p in _productRepository.Table
                                where p.Published && !p.Deleted && p.VisibleIndividually &&
                                     DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                                     DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                                select p;

            var productManufacturerQuery =
                     from pm in _productManufacturerRepository.Table
                     where pm.ManufacturerId == manufacturerId
                     group pm by pm.ProductId into pm
                     select new
                     {
                         ProductId = pm.Key,
                         DisplayOrder = pm.First().DisplayOrder
                     };

            productsQuery =
                from p in productsQuery
                join pm in productManufacturerQuery on p.Id equals pm.ProductId
                orderby pm.DisplayOrder, p.Name
                select p;

            return productsQuery;
        }
        else
        {
            if (product.VendorId == 0)
                return null;

            var productsQuery = from p in _productRepository.Table
                                join v in _vendorRepository.Table on p.VendorId equals v.Id
                                where !v.Deleted && p.Published && !p.Deleted && p.VisibleIndividually &&
                                     DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                                     DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                                orderby p.DisplayOrder, p.Name
                                select p;

            return productsQuery;
        }
    }

    #endregion

    #region Methods

    public async Task<(Product Previous, Product Next)> GetProductsAsync(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);

        var productsQuery = await GetProductsQueryAsync(product);
        if (productsQuery == null)
            return (null, null);

        //apply store mapping constraints
        productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, _storeContext.GetCurrentStore().Id);

        //apply ACL constraints
        var customer = await _workContext.GetCurrentCustomerAsync();
        productsQuery = await _aclService.ApplyAcl(productsQuery, customer);

        var count = await productsQuery.CountAsync();
        if (count == 0)
            return (null, null);

        var dictionary1 = productsQuery.Select((x, index) => new { Index = index + 1, ProductId = x.Id })
            .ToDictionary(t => t.ProductId, t => t.Index);
        var dictionary2 = productsQuery.Select((x, index) => new { Index = index + 1, ProductId = x.Id })
            .ToDictionary(t => t.Index, t => t.ProductId);

        if (!dictionary1.ContainsKey(productId))
            return (null, null);

        var ci = dictionary1[productId];
        var prevIndex = ci - 1;
        var nextIndex = ci + 1;

        if (prevIndex == 0 && _prevNextProductSettings.EnableLoop)
            prevIndex = count;
        if (nextIndex > count && _prevNextProductSettings.EnableLoop)
            nextIndex = 1;

        var prevId = !dictionary2.ContainsKey(prevIndex) ? 0 : dictionary2[prevIndex];
        var nextId = !dictionary2.ContainsKey(nextIndex) ? 0 : dictionary2[nextIndex];

        return (await _productService.GetProductByIdAsync(prevId), await _productService.GetProductByIdAsync(nextId));
    }

    #endregion
}
