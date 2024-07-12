using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;

public class QuoteWhitelistModelFactory : IQuoteWhitelistModelFactory
{
    #region Fields

    private readonly ICategoryService _categoryService;
    private readonly ILanguageService _languageService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductCategory> _productCategoryRepository;
    private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public QuoteWhitelistModelFactory(
        ICategoryService categoryService,
        ILanguageService languageService,
        IManufacturerService manufacturerService,
        IRepository<LocalizedProperty> localizedPropertyRepository,
        IRepository<Product> productRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductManufacturer> productManufacturerRepository,
        IStoreMappingService storeMappingService,
        IWorkContext workContext)
    {
        _categoryService = categoryService;
        _languageService = languageService;
        _manufacturerService = manufacturerService;
        _localizedPropertyRepository = localizedPropertyRepository;
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _productManufacturerRepository = productManufacturerRepository;
        _storeMappingService = storeMappingService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task<ProductListModel> PrepareAddProductListModelAsync(ProductSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var pageSize = searchModel.PageSize;
        var pageIndex = searchModel.Page - 1;

        //some databases don't support int.MaxValue
        if (pageSize == int.MaxValue)
            pageSize = int.MaxValue - 1;

        var productsQuery = _productRepository.Table.Where(p => p.ProductTypeId == (int)ProductType.SimpleProduct);

        await _storeMappingService.ApplyStoreMapping(productsQuery, searchModel.SearchStoreId);

        var keywords = searchModel.SearchProductName;

        if (!string.IsNullOrEmpty(keywords))
        {
            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            IQueryable<int> productsByKeywords;

            productsByKeywords =
                    from p in _productRepository.Table
                    where p.Name.Contains(keywords)
                    select p.Id;

            productsByKeywords = productsByKeywords.Union(
                from lp in _localizedPropertyRepository.Table
                let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                lp.LocaleValue.Contains(keywords)
                where
                    lp.LocaleKeyGroup == nameof(Product)

                select lp.EntityId);

            productsQuery =
                from p in productsQuery
                join pbk in productsByKeywords on p.Id equals pbk
                select p;
        }

        var categoryIds = new List<int> { searchModel.SearchCategoryId };

        if (categoryIds is not null)
        {
            categoryIds.Remove(0);

            if (categoryIds.Count != 0)
            {
                var productCategoryQuery =
                    from pc in _productCategoryRepository.Table
                    where categoryIds.Contains(pc.CategoryId)
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
            }
        }

        var manufacturerIds = new List<int> { searchModel.SearchManufacturerId };

        if (manufacturerIds is not null)
        {
            manufacturerIds.Remove(0);

            if (manufacturerIds.Count != 0)
            {
                var productManufacturerQuery =
                    from pm in _productManufacturerRepository.Table
                    where manufacturerIds.Contains(pm.ManufacturerId)
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
            }
        }

        var products = await productsQuery.OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), ProductSortingEnum.Position).ToPagedListAsync(pageIndex, pageSize);
        ;

        //prepare grid model
        var model = await new ProductListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                var productModel = product.ToModel<ProductModel>();
                return await Task.FromResult(productModel);
            });
        });

        return model;
    }


    /// <summary>
    /// Prepare paged category list model
    /// </summary>
    /// <param name="searchModel">Category search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the category list model
    /// </returns>
    public virtual async Task<CategoryListModel> PrepareCategoryListModelAsync(CategorySearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        //get categories
        var categories = await _categoryService.GetAllCategoriesAsync(categoryName: searchModel.SearchCategoryName,
            showHidden: true,
            storeId: searchModel.SearchStoreId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
            overridePublished: searchModel.SearchPublishedId == 0 ? null : searchModel.SearchPublishedId == 1);

        //prepare grid model
        var model = await new CategoryListModel().PrepareToGridAsync(searchModel, categories, () =>
        {
            return categories.SelectAwait(async category =>
            {
                //fill in model values from the entity
                var categoryModel = category.ToModel<CategoryModel>();
                categoryModel.Breadcrumb = await _categoryService.GetFormattedBreadCrumbAsync(category);
                return categoryModel;
            });
        });

        return model;
    }


    /// <summary>
    /// Prepare paged manufacturer list model
    /// </summary>
    /// <param name="searchModel">Manufacturer search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the manufacturer list model
    /// </returns>
    public virtual async Task<ManufacturerListModel> PrepareManufacturerListModelAsync(ManufacturerSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get manufacturers
        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(
            showHidden: true,
            manufacturerName: searchModel.SearchManufacturerName,
            storeId: searchModel.SearchStoreId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
            overridePublished: searchModel.SearchPublishedId == 0 ? null : searchModel.SearchPublishedId == 1);

        //prepare grid model
        var model = await new ManufacturerListModel().PrepareToGridAsync(searchModel, manufacturers, () =>
        {
            //fill in model values from the entity
            return manufacturers.SelectAwait(async manufacturer =>
            {
                var manufacturerModel = manufacturer.ToModel<ManufacturerModel>();

                return await Task.FromResult(manufacturerModel);
            });
        });

        return model;
    }

    #endregion
}
