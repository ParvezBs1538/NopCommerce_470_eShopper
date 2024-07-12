using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories
{
    public class CatalogCommissionModelFactory : ICatalogCommissionModelFactory
    {
        #region Fields

        private readonly ICatalogCommissionService _catalogCommissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly IManufacturerModelFactory _manufacturerModelFactory;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        #endregion

        #region Ctor

        public CatalogCommissionModelFactory(ICatalogCommissionService catalogCommissionService,
            ILocalizationService localizationService,
            IProductModelFactory productModelFactory,
            ICategoryModelFactory categoryModelFactory,
            IManufacturerModelFactory manufacturerModelFactory,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            CurrencySettings currencySettings)
        {
            _catalogCommissionService = catalogCommissionService;
            _localizationService = localizationService;
            _productModelFactory = productModelFactory;
            _categoryModelFactory = categoryModelFactory;
            _manufacturerModelFactory = manufacturerModelFactory;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
        }

        #endregion

        #region Utilities

        #region List

        protected async Task<CatalogCommissionListModel> PrepareProductCommissionListModelAsync(ProductSearchModel searchModel)
        {
            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);

            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                overridePublished: overridePublished);

            var model = await new CatalogCommissionListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    return await PrepareProductCommissionModelAsync(null, product);
                });
            });

            return model;
        }

        protected async Task<CatalogCommissionListModel> PrepareCategoryCommissionListModelAsync(CategorySearchModel searchModel)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(categoryName: searchModel.SearchCategoryName,
                showHidden: true,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = await new CatalogCommissionListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                return categories.SelectAwait(async category =>
                {
                    //fill in model values from the entity
                    return await PrepareCategoryCommissionModelAsync(null, category);
                });
            });

            return model;
        }

        protected async Task<CatalogCommissionListModel> PrepareManufacturerCommissionListModelAsync(ManufacturerSearchModel searchModel)
        {
            var manufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true,
                manufacturerName: searchModel.SearchManufacturerName,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new CatalogCommissionListModel().PrepareToGridAsync(searchModel, manufacturers, () =>
             {
                 return manufacturers.SelectAwait(async manufacturer =>
                 {
                     //fill in model values from the entity
                     return await PrepareManufacturerCommissionModelAsync(null, manufacturer);
                 });
             });

            return model;
        }

        #endregion

        #region Single

        protected async Task<CatalogCommissionModel> PrepareProductCommissionModelAsync(CatalogCommissionModel model, Product product)
        {
            if (model == null)
                model = new CatalogCommissionModel();

            model.EntityId = product.Id;
            model.EntityName = product.GetType().Name;
            model.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name);

            var commission = await _catalogCommissionService.GetCatalogCommissionByEntityAsync(product);
            await PrepareCommissionModelAsync(model, commission);

            return model;
        }

        protected async Task<CatalogCommissionModel> PrepareCategoryCommissionModelAsync(CatalogCommissionModel model, Category category)
        {
            if (model == null)
                model = new CatalogCommissionModel();

            model.EntityId = category.Id;
            model.EntityName = category.GetType().Name;
            model.Name = await _localizationService.GetLocalizedAsync(category, x => x.Name);

            var commission = await _catalogCommissionService.GetCatalogCommissionByEntityAsync(category);
            await PrepareCommissionModelAsync(model, commission);

            return model;
        }

        protected async Task<CatalogCommissionModel> PrepareManufacturerCommissionModelAsync(CatalogCommissionModel model, Manufacturer manufacturer)
        {
            if (model == null)
                model = new CatalogCommissionModel();

            model.EntityId = manufacturer.Id;
            model.EntityName = manufacturer.GetType().Name;
            model.Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name);

            var commission = await _catalogCommissionService.GetCatalogCommissionByEntityAsync(manufacturer);
            await PrepareCommissionModelAsync(model, commission);

            return model;
        }

        #endregion

        protected async Task PrepareCommissionModelAsync(CatalogCommissionModel model, CatalogCommission commission)
        {
            if (commission != null)
            {
                model.CommissionAmount = commission.CommissionAmount;
                model.CommissionPercentage = commission.CommissionPercentage;
                model.UsePercentage = commission.UsePercentage;
                model.Id = commission.Id;

                if (commission.UsePercentage)
                    model.Commission = string.Format("{0}{1}", commission.CommissionPercentage.ToString("0.##"), "%");
                else
                    model.Commission = await _priceFormatter.FormatPriceAsync(commission.CommissionAmount);
            }
        }

        #endregion

        #region Methods

        public async Task<CatalogCommissionSearchModel> PrepareCatalogCommissionSearchModelAsync(SearchType searchType)
        {
            var model = new CatalogCommissionSearchModel();

            if (searchType == SearchType.Product)
                model.ProductSearchModel = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());
            if (searchType == SearchType.Category)
                model.CategorySearchModel = await _categoryModelFactory.PrepareCategorySearchModelAsync(new CategorySearchModel());
            if (searchType == SearchType.Manufacturer)
                model.ManufacturerSearchModel = await _manufacturerModelFactory.PrepareManufacturerSearchModelAsync(new ManufacturerSearchModel());

            return model;
        }

        public async Task<CatalogCommissionListModel> PrepareCatalogCommissionListModelAsync(CatalogCommissionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (searchModel.SearchType == SearchType.Product)
                return await PrepareProductCommissionListModelAsync(searchModel.ProductSearchModel);
            if (searchModel.SearchType == SearchType.Category)
                return await PrepareCategoryCommissionListModelAsync(searchModel.CategorySearchModel);
            if (searchModel.SearchType == SearchType.Manufacturer)
                return await PrepareManufacturerCommissionListModelAsync(searchModel.ManufacturerSearchModel);

            throw new ArgumentNullException(nameof(searchModel));
        }

        public async Task<CatalogCommissionModel> PrepareCatalogCommissionModelAsync(CatalogCommissionModel model, BaseEntity baseEntity)
        {
            if (baseEntity == null)
                throw new ArgumentNullException(nameof(baseEntity));

            var entityName = baseEntity.GetType().Name;

            switch (entityName)
            {
                case "Category":
                    model = await PrepareCategoryCommissionModelAsync(model, baseEntity as Category);
                    model.ViewPath = "CategoryEdit";
                    break;
                case "Product":
                    model = await PrepareProductCommissionModelAsync(model, baseEntity as Product);
                    model.ViewPath = "ProductEdit";
                    break;
                case "Manufacturer":
                    model = await PrepareManufacturerCommissionModelAsync(model, baseEntity as Manufacturer);
                    model.ViewPath = "ManufacturerEdit";
                    break;
                default:
                    throw new ArgumentNullException(nameof(baseEntity));
            }

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            return model;
        }

        #endregion
    }
}
