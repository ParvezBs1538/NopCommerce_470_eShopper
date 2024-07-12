using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services;
using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;
using NopStation.Plugin.Misc.AjaxFilter.Services;

namespace NopStation.Plugin.Misc.AjaxFilter.Factories
{
    public class AjaxFilterCatalogModelFactory : CatalogModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IAjaxFilterSpecificationAttributeService _ajaxFilterSpecificationAttributeService;
        private readonly IAjaxFilterService _ajaxFilterService;

        #endregion

        #region Ctor

        public AjaxFilterCatalogModelFactory(BlogSettings blogSettings,
        CatalogSettings catalogSettings,
        DisplayDefaultMenuItemSettings displayDefaultMenuItemSettings,
        ForumSettings forumSettings,
        ICategoryService categoryService,
        ICategoryTemplateService categoryTemplateService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IEventPublisher eventPublisher,
        IHttpContextAccessor httpContextAccessor,
        IJsonLdModelFactory jsonLdModelFactory,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        IManufacturerTemplateService manufacturerTemplateService,
        INopUrlHelper nopUrlHelper,
        IPictureService pictureService,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IProductTagService productTagService,
        ISearchTermService searchTermService,
        ISpecificationAttributeService specificationAttributeService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        ITopicService topicService,
        IUrlRecordService urlRecordService,
        IVendorService vendorService,
        IWebHelper webHelper,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        SeoSettings seoSettings,
        VendorSettings vendorSettings,
        ISettingService settingService,
        IAjaxFilterSpecificationAttributeService ajaxFilterSpecificationAttributeService,
        IAjaxFilterService ajaxFilterService) : base(blogSettings,
            catalogSettings,
            displayDefaultMenuItemSettings,
            forumSettings,
            categoryService,
            categoryTemplateService,
            currencyService,
            customerService,
            eventPublisher,
            httpContextAccessor,
            jsonLdModelFactory,
            localizationService,
            manufacturerService,
            manufacturerTemplateService,
            nopUrlHelper,
            pictureService,
            productModelFactory,
            productService,
            productTagService,
            searchTermService,
            specificationAttributeService,
            staticCacheManager,
            storeContext,
            topicService,
            urlRecordService,
            vendorService,
            webHelper,
            workContext,
            mediaSettings,
            seoSettings,
            vendorSettings)
        {
            _settingService = settingService;
            _ajaxFilterSpecificationAttributeService = ajaxFilterSpecificationAttributeService;
            _ajaxFilterService = ajaxFilterService;
        }

        #endregion

        #region Methods

        public override async Task PrepareSortingOptionsAsync(CatalogProductsModel model, CatalogProductsCommand command)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ajaxFilterSettings = await _settingService.LoadSettingAsync<AjaxFilterSettings>(storeScope);

            //set the order by position by default
            model.OrderBy = command.OrderBy;
            command.OrderBy = (int)ProductSortingEnum.Position;

            //ensure that product sorting is enabled
            if (!_catalogSettings.AllowProductSorting)
                return;

            //get active sorting options
            var activeSortingOptionsIds = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled).ToList();

            if (!activeSortingOptionsIds.Any())
                return;

            //order sorting options
            var orderedActiveSortingOptions = activeSortingOptionsIds
                .Select(id => new { Id = id, Order = _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(id, out var order) ? order : id })
                .OrderBy(option => option.Order).ToList();

            model.AllowProductSorting = true;
            command.OrderBy = model.OrderBy ?? orderedActiveSortingOptions.FirstOrDefault().Id;

            //prepare available model sorting options
            foreach (var option in orderedActiveSortingOptions)
            {
                model.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync((ProductSortingEnum)option.Id),
                    Value = option.Id.ToString(),
                    Selected = option.Id == command.OrderBy
                });
            }

            if (ajaxFilterSettings.EnableFilter)
            {
                model.AvailableSortOptions.RemoveAt(model.AvailableSortOptions.Count - 1);
                model.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.AjaxFilter.Filters.CreatedOnAsc"),
                    Value = ((int)AjaxFilterProductSortingEnum.CreatedOnAsc).ToString(),
                    Selected = command.OrderBy == (int)AjaxFilterProductSortingEnum.CreatedOnAsc
                });

                model.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.AjaxFilter.Filters.CreatedOnDesc"),
                    Value = ((int)AjaxFilterProductSortingEnum.CreatedOnDesc).ToString(),
                    Selected = command.OrderBy == (int)AjaxFilterProductSortingEnum.CreatedOnDesc
                });
            }
        }

        public virtual async Task<CatalogProductsModel> PrepareCategoryProductsModelAsync_Test(Category category, CatalogProductsCommand command)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var model = new CatalogProductsModel
            {
                UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
            };

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            //sorting
            await PrepareSortingOptionsAsync(model, command);

            //view mode
            await PrepareViewModesAsync(model, command);

            //page size
            await PreparePageSizeOptionsAsync(model, command, category.AllowCustomersToSelectPageSize,
                category.PageSizeOptions, category.PageSize);

            var categoryIds = new List<int> { category.Id };

            //include subcategories
            if (_catalogSettings.ShowProductsFromSubcategories)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));

            //price range
            PriceRangeModel selectedPriceRange = null;
            if (_catalogSettings.EnablePriceRangeFiltering && category.PriceRangeFiltering)
            {
                selectedPriceRange = await GetConvertedPriceRangeAsync(command);

                PriceRangeModel availablePriceRange = null;
                if (!category.ManuallyPriceRange)
                {
                    async Task<decimal?> getProductPriceAsync(ProductSortingEnum orderBy)
                    {
                                    var products = await _productService.SearchProductsAsync(0, 1,
                                        categoryIds: categoryIds,
                                        storeId: currentStore.Id,
                                        visibleIndividuallyOnly: true,
                                        excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                                        orderBy: orderBy);

                                    return products?.FirstOrDefault()?.Price ?? 0;
                    }

                    availablePriceRange = new PriceRangeModel
                    {
                                    From = await getProductPriceAsync(ProductSortingEnum.PriceAsc),
                                    To = await getProductPriceAsync(ProductSortingEnum.PriceDesc)
                    };
                }
                else
                {
                    availablePriceRange = new PriceRangeModel
                    {
                                    From = category.PriceFrom,
                                    To = category.PriceTo
                    };
                }

                model.PriceRangeFilter = await PreparePriceRangeFilterAsync(selectedPriceRange, availablePriceRange);
            }

            #region Custom Code

            var ajaxFilterSpecificationAttributes = (await _ajaxFilterSpecificationAttributeService.GetAvailableSpecificationAttributesAsync()).Select(x => x.SpecificationId);

            //filterable options
            var filterableOptions = await _specificationAttributeService.GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(category.Id);
            filterableOptions = filterableOptions.Where(x => ajaxFilterSpecificationAttributes.Contains(x.SpecificationAttributeId)).ToList();
            model.SpecificationFilter = await PrepareSpecificationFilterModel(category.Id, command.SpecificationOptionIds, filterableOptions);

            #endregion

            //filterable manufacturers
            if (_catalogSettings.EnableManufacturerFiltering)
            {
                var manufacturers = await _manufacturerService.GetManufacturersByCategoryIdAsync(category.Id);

                #region Custom Code

                model.ManufacturerFilter = await PrepareCurrentCategoryManufacturerFilterModel(category.Id, command.ManufacturerIds, manufacturers);

                #endregion
            }

            var filteredSpecs = command.SpecificationOptionIds is null ? null : filterableOptions.Where(fo => command.SpecificationOptionIds.Contains(fo.Id)).ToList();

            //products
            var products = await _productService.SearchProductsAsync(
                command.PageNumber - 1,
                command.PageSize,
                categoryIds: categoryIds,
                storeId: currentStore.Id,
                visibleIndividuallyOnly: true,
                excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                priceMin: selectedPriceRange?.From,
                priceMax: selectedPriceRange?.To,
                manufacturerIds: command.ManufacturerIds,
                filteredSpecOptions: filteredSpecs,
                orderBy: (ProductSortingEnum)command.OrderBy);

            #region Custom Code

            model.CustomProperties["ProductCount"] = products.Count.ToString();

            #endregion

            var isFiltering = filterableOptions.Any() || selectedPriceRange?.From is not null;
            await PrepareCatalogProductsAsync(model, products, isFiltering);

            return model;
        }

        protected virtual async Task<ManufacturerFilterModel> PrepareCurrentCategoryManufacturerFilterModel(int categoryId, IList<int> selectedManufacturers, IList<Manufacturer> availableManufacturers)
        {
            var model = new ManufacturerFilterModel();

            if (availableManufacturers?.Any() == true)
            {
                model.Enabled = true;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                foreach (var manufacturer in availableManufacturers)
                {
                    var manufacturerName = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name, workingLanguage.Id);
                    model.Manufacturers.Add(new SelectListItem
                    {
                                    Value = manufacturer.Id.ToString(),
                                    Text = manufacturerName,
                                    Selected = selectedManufacturers?
                                        .Any(manufacturerId => manufacturerId == manufacturer.Id) == true
                    });
                    var manufacturerIds = new List<int> { manufacturer.Id };
                    model.CustomProperties[$"{manufacturerName}-{manufacturer.Id}"] = (await _ajaxFilterService.GetNumberOfProductsInManufacturerAsync(categoryId, manufacturerIds, (await _storeContext.GetCurrentStoreAsync()).Id)).ToString();
                }
            }

            return model;
        }

        protected virtual async Task<SpecificationFilterModel> PrepareSpecificationFilterModel(int categoryId, IList<int> selectedOptions, IList<SpecificationAttributeOption> availableOptions)
        {
            var model = new SpecificationFilterModel();

            if (availableOptions?.Any() == true)
            {
                model.Enabled = true;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                foreach (var option in availableOptions)
                {
                    var attributeFilter = model.Attributes.FirstOrDefault(model => model.Id == option.SpecificationAttributeId);
                    if (attributeFilter == null)
                    {
                                    var attribute = await _specificationAttributeService
                                        .GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                                    attributeFilter = new SpecificationAttributeFilterModel
                                    {
                                        Id = attribute.Id,
                                        Name = await _localizationService
                                            .GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id)
                                    };
                                    model.Attributes.Add(attributeFilter);
                    }

                    var attributeOptionName = await _localizationService.GetLocalizedAsync(option, x => x.Name, workingLanguage.Id);
                    var attributeValue = new SpecificationAttributeValueFilterModel
                    {
                                    Id = option.Id,
                                    Name = attributeOptionName,
                                    Selected = selectedOptions?.Any(optionId => optionId == option.Id) == true,
                                    ColorSquaresRgb = option.ColorSquaresRgb
                    };

                    model.CustomProperties[$"{attributeOptionName}-{option.Id}"] = (await _ajaxFilterService.GetNumberOfProductsUsingSpecificationAttributeAsync(categoryId, option.Id, (await _storeContext.GetCurrentStoreAsync()).Id)).ToString();

                    attributeFilter.Values.Add(attributeValue);
                }
            }

            return model;
        }

        public override async Task<CategoryNavigationModel> PrepareCategoryNavigationModelAsync(int currentCategoryId, int currentProductId)
        {
            //get active category
            var activeCategoryId = 0;
            if (currentCategoryId > 0)
            {
                //category details page
                activeCategoryId = currentCategoryId;
            }
            else if (currentProductId > 0)
            {
                //product details page
                var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(currentProductId);
                if (productCategories.Any())
                    activeCategoryId = productCategories[0].CategoryId;
            }

            var parentCategoryId = await GetParentCategoryIdAsync(activeCategoryId);

            var category = await _categoryService.GetCategoryByIdAsync(parentCategoryId);
            var categoryModel = new CategorySimpleModel();
            if (category != null)
            {
                categoryModel = new CategorySimpleModel
                {
                    Id = parentCategoryId,
                    Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(category),
                    IncludeInTopMenu = category.IncludeInTopMenu
                };
                categoryModel.SubCategories = await GetCategorySimpleModelsAsync(parentCategoryId);

            }

            var model = new CategoryNavigationModel
            {
                CurrentCategoryId = activeCategoryId,
                Categories = new List<CategorySimpleModel> { categoryModel }
            };
            return model;
        }

        public virtual async Task<List<CategorySimpleModel>> GetCategorySimpleModelsAsync(int categoryId)
        {
            //load and cache them
            var language = await _workContext.GetWorkingLanguageAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AjaxFilterDefaults.CategoryModelKey,
                language, customerRoleIds, store, categoryId);

            return await _staticCacheManager.GetAsync(cacheKey, async () => await PrepareCategorySimpleModelsAsync(categoryId));
        }

        public async Task<int> GetParentCategoryIdAsync(int categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null || category.ParentCategoryId == 0)
            {
                return categoryId;
            }

            while (category.ParentCategoryId > 0)
            {
                category = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId);
            }

            return category.Id;
        }

        #endregion
    }
}
