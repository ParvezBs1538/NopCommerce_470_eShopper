using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services;
using NopStation.Plugin.Misc.AjaxFilter.Domains.Enums;
using NopStation.Plugin.Misc.AjaxFilter.Extensions;
using NopStation.Plugin.Misc.AjaxFilter.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Factories
{
    public class AjaxSearchModelFactory : IAjaxSearchModelFactory
    {
        #region Fields 

        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IAjaxFilterSpecificationAttributeService _ajaxFilterSpecificationAttributeService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly CatalogSettings _catalogSettings;
        private readonly AjaxFilterSettings _ajaxFilterSettings;
        private readonly IWorkContext _workContext;
        private readonly CurrencySettings _currencySettings;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductTagService _productTagService;
        private readonly IStoreContext _storeContext;

        #endregion


        #region Ctor 

        public AjaxSearchModelFactory(ICurrencyService currencyService, ILocalizationService localizationService, IAjaxFilterSpecificationAttributeService ajaxFilterSpecificationAttributeService, IProductService productService, ICategoryService categoryService, CatalogSettings catalogSettings, AjaxFilterSettings ajaxFilterSettings, IWorkContext workContext, CurrencySettings currencySettings, IManufacturerService manufacturerService, IVendorService vendorService, ISpecificationAttributeService specificationAttributeService, IProductAttributeService productAttributeService, IProductTagService productTagService, IStoreContext storeContext)
        {
            _currencyService = currencyService;
            _localizationService = localizationService;
            _ajaxFilterSpecificationAttributeService = ajaxFilterSpecificationAttributeService;
            _productService = productService;
            _categoryService = categoryService;
            _catalogSettings = catalogSettings;
            _ajaxFilterSettings = ajaxFilterSettings;
            _workContext = workContext;
            _currencySettings = currencySettings;
            _manufacturerService = manufacturerService;
            _vendorService = vendorService;
            _specificationAttributeService = specificationAttributeService;
            _productAttributeService = productAttributeService;
            _productTagService = productTagService;
            _storeContext = storeContext;
        }

        #endregion


        #region Methods 

        public async Task<PublicInfoModel> PreparePublicInfoModelAsync(int categoryId, int manufacturer, RouteValueDictionary routeValues, List<RequestParams> requestParams)
        {
            var publicInfoModel = new PublicInfoModel();

            var selectedPageNumber = requestParams.FirstOrDefault(x => x.Key.Equals("pagenumber", StringComparison.InvariantCultureIgnoreCase))?.Value;
            int.TryParse(selectedPageNumber, out var pageNumber);

            var pagesize = requestParams.FirstOrDefault(x => x.Key.Equals("pagesize", StringComparison.InvariantCultureIgnoreCase))?.Value;
            int.TryParse(pagesize, out var selectedPagesize);

            if (selectedPagesize == 0)
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category != null)
                {
                    if (!category.AllowCustomersToSelectPageSize)
                    {
                        selectedPagesize = category.PageSize;
                    }
                    else
                    {
                        var pageSizes = category.PageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pageSizes.Any())
                        {
                            int.TryParse(pageSizes.FirstOrDefault(), out selectedPagesize);
                        }
                    }
                }
                if (selectedPagesize == 0)
                {
                    var pageSizes = _catalogSettings.DefaultCategoryPageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pageSizes.Any())
                    {
                        int.TryParse(pageSizes.FirstOrDefault(), out selectedPagesize);
                    }
                }
            }
            publicInfoModel.PageSize = selectedPagesize;

            var viewMode = _catalogSettings.DefaultViewMode;
            var selectedViewmode = requestParams.FirstOrDefault(x => x.Key.Equals("viewmode", StringComparison.InvariantCultureIgnoreCase))?.Value;
            if (selectedViewmode != null)
            {
                viewMode = selectedViewmode;
            }

            publicInfoModel.ViewMode = viewMode;

            var orderBy = 0;
            var selectedOrderBy = requestParams.FirstOrDefault(x => x.Key.Equals("orderby", StringComparison.InvariantCultureIgnoreCase))?.Value;
            if (selectedOrderBy != null)
            {
                int.TryParse(selectedOrderBy, out orderBy);
            }
            publicInfoModel.SortOption = orderBy;
            publicInfoModel.SpecificationOptionIds = requestParams.FirstOrDefault(x => x.Key.Equals("specs", StringComparison.InvariantCultureIgnoreCase))?.Value;
            publicInfoModel.ManufacturerIds = requestParams.FirstOrDefault(x => x.Key.Equals("ms", StringComparison.InvariantCultureIgnoreCase))?.Value;
            publicInfoModel.ProductAttributeOptionIds = requestParams.FirstOrDefault(x => x.Key.Equals("pattrs", StringComparison.InvariantCultureIgnoreCase))?.Value;
            publicInfoModel.ProductTagIds = requestParams.FirstOrDefault(x => x.Key.Equals("tagids", StringComparison.InvariantCultureIgnoreCase))?.Value;
            publicInfoModel.DiscountedProduct = !string.IsNullOrEmpty(requestParams.FirstOrDefault(x => x.Key.Equals("discountedProduct", StringComparison.InvariantCultureIgnoreCase))?.Value);
            publicInfoModel.FreeShipping = !string.IsNullOrEmpty(requestParams.FirstOrDefault(x => x.Key.Equals("freeShipping", StringComparison.InvariantCultureIgnoreCase))?.Value);
            publicInfoModel.NewProduct = !string.IsNullOrEmpty(requestParams.FirstOrDefault(x => x.Key.Equals("newProduct", StringComparison.InvariantCultureIgnoreCase))?.Value);
            publicInfoModel.TaxExempt = !string.IsNullOrEmpty(requestParams.FirstOrDefault(x => x.Key.Equals("taxExempt", StringComparison.InvariantCultureIgnoreCase))?.Value);


            return publicInfoModel;
        }

        public async Task<PublicInfoModel> CompletePublicInfoModelAsync(PublicInfoModel model, SearchFilterResult query)
        {
            model.EnableFilter = _ajaxFilterSettings.EnableFilter;

            if (_ajaxFilterSettings.EnablePriceRangeFilter)
            {
                model.EnablePriceRangeFilter = true;
                var resultPrice = query.PriceRange;
                if (resultPrice != null)
                {
                    var d = decimal.One;
                    if ((await _workContext.GetWorkingCurrencyAsync()).Id != _currencySettings.PrimaryStoreCurrencyId)
                    {
                        d = (await _workContext.GetWorkingCurrencyAsync()).Rate;
                    }
                    model.FilterPriceModel = new FilterPriceRangeModel
                    {
                        MinPrice = resultPrice.PriceMin * d,
                        MaxPrice = resultPrice.PriceMax * d
                    };

                    var priceRange = await GetConvertedPriceRangeAsync(model.FilteredPrice);

                    model.FilterPriceModel.CurrentMinPrice = ((priceRange.MinPrice == decimal.Zero) ? model.FilterPriceModel.MinPrice : priceRange.MinPrice);
                    model.FilterPriceModel.CurrentMaxPrice = ((priceRange.MaxPrice == decimal.Zero) ? model.FilterPriceModel.MaxPrice : priceRange.MaxPrice);
                    model.FilterPriceModel.CurrencySymbol = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
                }
                if (model.FilterPriceModel != null)
                {
                    if (Convert.ToInt32(model.FilterPriceModel.MinPrice) == Convert.ToInt32(model.FilterPriceModel.MaxPrice))
                    {
                        model.EnablePriceRangeFilter = false;
                    }
                }
                else
                {
                    model.EnablePriceRangeFilter = false;
                }
            }

            if (_ajaxFilterSettings.EnableManufacturerFilter && model.ManufacturerId == 0)
            {
                model.EnableManufacturersFilter = true;

                var manufacturers = query.Manufacturers;
                if (manufacturers != null && manufacturers.Count > 0)
                {
                    var selectedManufacturerIds = AjaxFilterHelper.ConvertToIntegerList(model.ManufacturerIds);
                    var availableManufacturers = await _manufacturerService.GetManufacturersByIdsAsync(manufacturers.Select(x => x.Id).ToArray());
                    model.ManufacturerModel = await PrepareManufacturerFilterModel(selectedManufacturerIds, availableManufacturers, manufacturers);
                }
            }

            if (_ajaxFilterSettings.EnableProductTagsFilter)
            {
                model.EnableProductTagsFilter = true;

                var productTags = query.ProductTags;

                if (productTags != null && productTags.Count > 0)
                {
                    var selectedTagIds = AjaxFilterHelper.ConvertToIntegerList(model.ProductTagIds);
                    var availableTags = await _productTagService.GetProductTagsByIdsAsync(productTags.Select(x => x.Id).ToArray());
                    model.ProductTagsModel = await PrepareProductTagFilterModel(selectedTagIds, availableTags, productTags);
                }

            }

            if (_ajaxFilterSettings.EnableProductRatingsFilter)
            {
                model.EnableProductRating = true;
                var ratings = query.Ratings;
                
                var filterRatingModel = new FilterRatingModel();
                filterRatingModel.ProductRatingIds = model.ProductRatingIds;
                for (int i = 1; i <= 5; i++)
                {
                    filterRatingModel.Ratings.Add(new RatingModel
                    {
                        Id = i,
                        Name = i.ToString(),
                        Count = ratings.FirstOrDefault(x => x.Id == i)?.Count ?? 0

                    });
                }
                model.FilterRatingModel = filterRatingModel;               
            }

            if (_ajaxFilterSettings.EnableVendorFilter)
            {
                model.EnableVendorsFilter = true;
                var vendors = query.Vendors;
                if (vendors != null && vendors.Count > 0)
                {
                    var selectedVendorIds = AjaxFilterHelper.ConvertToIntegerList(model.VendorIds);
                    var availableVendors = await _vendorService.GetAllVendorsAsync();
                    model.VendorsModel = await PrepareVendorFilterModel(selectedVendorIds, availableVendors, vendors);
                }
            }

            if (_ajaxFilterSettings.EnableMiscFilter)
            {
                model.EnableMiscFilter = true;
                model.FilterProductsModel = await PrepareProductFilterModel(model.TaxExempt, model.FreeShipping, model.NewProduct, model.DiscountedProduct);
            }

            if (_ajaxFilterSettings.EnableSpecificationAttributeFilter)
            {
                model.EnableSpecificationsFilter = true;
                var specifications = query.Specifications;
                if (specifications != null && specifications.Count > 0)
                {
                    var selectedSpecificationOptionIds = AjaxFilterHelper.ConvertToIntegerList(model.SpecificationOptionIds);
                    model.SpecificationModel = new FilterSpecificationsModel
                    {
                        MaxDisplayForSpecificationAttributes = int.MaxValue,
                        CheckOrDropdowns = (FiltersUI)_ajaxFilterSettings.SpecificationFilterDisplayTypeId,
                    };
                    var filterableOptions = specifications;

                    var ajaxFilterSpecificationAttributes = (await _ajaxFilterSpecificationAttributeService.GetAvailableSpecificationAttributesAsync()).Select(x => x.SpecificationId);

                    if (ajaxFilterSpecificationAttributes.Any())
                    {
                        filterableOptions = filterableOptions.Where(x => ajaxFilterSpecificationAttributes.Contains(x.SpecificationAttributeId)).ToList();
                    }

                    model.SpecificationModel = await PrepareSpecificationFilterModel(selectedSpecificationOptionIds, filterableOptions, specifications);
                }
            }

            if (_ajaxFilterSettings.EnableProductAttributeFilter)
            {
                model.EnableProductAttributeFilter = true;
                var attributes = query.ProductAttributes;
                var selectedAttributes = new List<string>();
                if (!string.IsNullOrEmpty(model.ProductAttributeOptionIds))
                    selectedAttributes = model.ProductAttributeOptionIds.Split(",").ToList();
                if (attributes != null && attributes.Count > 0)
                {
                    model.AttributesModel = new FilterProductAttributesModel
                    {
                        CheckOrDropdowns = _ajaxFilterSettings.ProductAttributeFilterDisplayTypeId
                    };
                    foreach (var attr in attributes.GroupBy(x => x.ProductAttributeId).Select(x => x.Key))
                    {
                        var filterProductVariantAttributesModel = new FilterProductVariantAttributesModel();
                        var productAttributeById = await _productAttributeService.GetProductAttributeByIdAsync(attr);
                        filterProductVariantAttributesModel.Id = attr;
                        filterProductVariantAttributesModel.Name = productAttributeById.Name;
                        foreach (var item3 in attributes.Where(x => x.ProductAttributeId == attr))
                        {
                            filterProductVariantAttributesModel.ProductVariantAttributesOptions.Add(new ProductVariantAttributesOptionsModel
                            {
                                Name = item3.Name,
                                Count = item3.Count,
                                CheckedState = selectedAttributes.Any(s => s == $"{item3.ProductAttributeId}-{item3.Name}") ? CheckedState.Checked : CheckedState.UnChecked
                            });
                        }
                        model.AttributesModel.ProductVariantAttributes.Add(filterProductVariantAttributesModel);
                    }
                }
            }

            if (query.Products != null)
            {
                model.Count = query.Products.TotalCount;
            }
            else
            {
                model.Count = await _productService.GetNumberOfProductsInCategoryAsync(new List<int> { model.CategoryId });
            }

            return model;
        }

        public async Task<SearchModel> PrepareSearchModelAsync(PublicInfoModel publicInfoModel, string typ = "")
        {
            var searchModel = new SearchModel();
            var filterUrl = $"{publicInfoModel.RequestPath}?viewmode={publicInfoModel.ViewMode}&orderby={publicInfoModel.SortOption}&pagesize={publicInfoModel.PageSize}&pagenumber={publicInfoModel.PageNumber}";

            searchModel.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
            if (publicInfoModel.CategoryId != 0)
            {
                searchModel.CategoryIds.Add(publicInfoModel.CategoryId);
                //add all child category
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(publicInfoModel.CategoryId);
                if (childCategoryIds.Count > 0)
                {
                    foreach (var childCategoryId in childCategoryIds)
                    {
                        searchModel.CategoryIds.Add(childCategoryId);
                    }
                }
            }
            searchModel.FilterBy = publicInfoModel.SearchElementName == null ? "" : publicInfoModel.SearchElementName;
            searchModel.ShowHidden = false;
            searchModel.VisibleIndividuallyOnly = true;
            searchModel.PageSize = publicInfoModel.PageSize;
            searchModel.PageIndex = publicInfoModel.PageNumber;
            searchModel.OrderBy = (ProductSortingEnum)publicInfoModel.SortOption;

            if (!string.IsNullOrEmpty(publicInfoModel.FilteredPrice))
            {
                var price = await GetConvertedPriceRangeAsync(publicInfoModel.FilteredPrice);

                searchModel.PriceMin = price.MinPrice;
                searchModel.PriceMax = price.MaxPrice;

                filterUrl += $"&price={publicInfoModel.FilteredPrice}";
            }

            if (!string.IsNullOrEmpty(publicInfoModel.ManufacturerIds))
            {
                searchModel.ManufacturerIds = AjaxFilterHelper.ConvertToIntegerList(publicInfoModel.ManufacturerIds);
                filterUrl += $"&ms={publicInfoModel.ManufacturerIds}";
            }

            if (!string.IsNullOrEmpty(publicInfoModel.ProductTagIds))
            {
                searchModel.ProductTagIds = AjaxFilterHelper.ConvertToIntegerList(publicInfoModel.ProductTagIds);
                filterUrl += $"&tagids={publicInfoModel.ProductTagIds}";
            }

            if (!string.IsNullOrEmpty(publicInfoModel.SpecificationOptionIds))
            {
                searchModel.FilteredSpecs = AjaxFilterHelper.ConvertToIntegerList(publicInfoModel.SpecificationOptionIds);
                filterUrl += $"&specs={publicInfoModel.SpecificationOptionIds}";
            }

            if (!string.IsNullOrEmpty(publicInfoModel.ProductAttributeOptionIds))
            {
                searchModel.FilteredProductAttributes = publicInfoModel.ProductAttributeOptionIds;
                filterUrl += $"&pattrs={publicInfoModel.ProductAttributeOptionIds}";
            }

            if (!string.IsNullOrEmpty(publicInfoModel.VendorIds))
            {
                searchModel.VendorIds = AjaxFilterHelper.ConvertToIntegerList(publicInfoModel.VendorIds);
                filterUrl += $"&vendors={publicInfoModel.VendorIds}";
            }

            if (publicInfoModel.FreeShipping)
            {
                searchModel.FilterFreeShipping = publicInfoModel.FreeShipping;
                filterUrl += $"&freeShipping={publicInfoModel.FreeShipping}";
            }

            if (publicInfoModel.TaxExempt)
            {
                searchModel.FilterTaxExempt = publicInfoModel.TaxExempt;
                filterUrl += $"&taxExempt={publicInfoModel.TaxExempt}";
            }

            if (publicInfoModel.DiscountedProduct)
            {
                searchModel.FilterDiscountedProduct = publicInfoModel.DiscountedProduct;
                filterUrl += $"&discountedProduct={publicInfoModel.DiscountedProduct}";
            }

            if (publicInfoModel.NewProduct)
            {
                searchModel.FilterNewProduct = publicInfoModel.NewProduct;
                filterUrl += $"&newProduct={publicInfoModel.NewProduct}";
            }

            if (!string.IsNullOrEmpty(publicInfoModel.ProductRatingIds))
            {
                searchModel.ProductRatingIds = AjaxFilterHelper.ConvertToIntegerList(publicInfoModel.ProductRatingIds);
                filterUrl += $"&productReview={publicInfoModel.ProductRatingIds}";
            }

            searchModel.Url = filterUrl;
            return searchModel;
        }

        public virtual Task<FilterPriceRangeModel> GetConvertedPriceRangeAsync(string price)
        {
            var result = new FilterPriceRangeModel();

            if (string.IsNullOrWhiteSpace(price))
                return Task.FromResult(result);

            var fromTo = price.Trim().Split(new[] { '-' });
            if (fromTo.Length == 2)
            {
                var rawFromPrice = fromTo[0]?.Trim();
                if (!string.IsNullOrEmpty(rawFromPrice) && decimal.TryParse(rawFromPrice, out var from))
                    result.MinPrice = from;

                var rawToPrice = fromTo[1]?.Trim();
                if (!string.IsNullOrEmpty(rawToPrice) && decimal.TryParse(rawToPrice, out var to))
                    result.MaxPrice = to;

                if (result.MinPrice > result.MaxPrice)
                    result.MinPrice = result.MaxPrice;
            }

            return Task.FromResult(result);
        }

        protected virtual async Task<FilterVendorsModel> PrepareVendorFilterModel(IList<int> selectedVendors, IList<Vendor> availableVendors, IList<SearchFilterResult.Vendor> vendorsResult)
        {
            var model = new FilterVendorsModel();

            if (availableVendors?.Any() == true)
            {
                model.CheckOrDropdown = (FiltersUI)_ajaxFilterSettings.ManufacturerFilterDisplayTypeId;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                foreach (var vendor in availableVendors)
                {
                    model.Vendors.Add(new VendorsModel
                    {
                        Id = vendor.Id,
                        Name = await _localizationService
                            .GetLocalizedAsync(vendor, x => x.Name, workingLanguage.Id),
                        CheckedState = selectedVendors?
                            .Any(vendorsId => vendorsId == vendor.Id) == true ? CheckedState.Checked : CheckedState.UnChecked,

                    });
                }
            }

            return model;
        }

        protected virtual async Task<FilterManufacturersModel> PrepareManufacturerFilterModel(IList<int> selectedManufacturers, IList<Manufacturer> availableManufacturers, IList<SearchFilterResult.Manufacturer> manufacturersResult)
        {
            var model = new FilterManufacturersModel();

            if (availableManufacturers?.Any() == true)
            {
                model.CheckOrDropdown = (FiltersUI)_ajaxFilterSettings.ManufacturerFilterDisplayTypeId;
                model.MaxManufacturersToDisplay = _ajaxFilterSettings.MaxDisplayForManufacturers;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                foreach (var manufacturer in availableManufacturers)
                {
                    model.Manufacturers.Add(new ManufacturersModel
                    {
                        Id = manufacturer.Id,
                        Name = await _localizationService
                            .GetLocalizedAsync(manufacturer, x => x.Name, workingLanguage.Id),
                        CheckedState = selectedManufacturers?
                            .Any(manufacturerId => manufacturerId == manufacturer.Id) == true ? CheckedState.Checked : CheckedState.UnChecked,
                        Count = manufacturersResult.FirstOrDefault(x => x.Id == manufacturer.Id).Count
                    });
                }
            }

            return model;
        }

        protected virtual async Task<FilterProductTagsModel> PrepareProductTagFilterModel(IList<int> selectedTags, IList<ProductTag> availableTags, IList<SearchFilterResult.ProductTag> productTagsResult)
        {
            var model = new FilterProductTagsModel();

            if (availableTags?.Any() == true)
            {
                model.CheckOrDropdown = (FiltersUI)_ajaxFilterSettings.ManufacturerFilterDisplayTypeId;
                model.MaxManufacturersToDisplay = _ajaxFilterSettings.MaxDisplayForManufacturers;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                foreach (var tag in availableTags)
                {
                    model.ProductTags.Add(new ProductTagsModel
                    {
                        Id = tag.Id,
                        Name = await _localizationService
                            .GetLocalizedAsync(tag, x => x.Name, workingLanguage.Id),
                        CheckedState = selectedTags?
                            .Any(tagId => tagId == tag.Id) == true ? CheckedState.Checked : CheckedState.UnChecked,
                        Count = productTagsResult.FirstOrDefault(x => x.Id == tag.Id).Count
                    });
                }
            }

            return model;
        }

        protected virtual async Task<FilterSpecificationsModel> PrepareSpecificationFilterModel(IList<int> selectedOptions, IList<SearchFilterResult.Specification> availableOptions, IList<SearchFilterResult.Specification> specificationResult)
        {
            var model = new FilterSpecificationsModel();

            if (availableOptions?.Any() == true)
            {
                model.CheckOrDropdowns = (FiltersUI)_ajaxFilterSettings.SpecificationFilterDisplayTypeId;
                model.MaxDisplayForSpecificationAttributes = int.MaxValue;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                foreach (var option in availableOptions)
                {
                    var attributeFilter = model.SpecificationAttributes.FirstOrDefault(model => model.Id == option.SpecificationAttributeId);
                    if (attributeFilter == null)
                    {
                        var attribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                        var ajaxFilterSpecificationAttribute = await _ajaxFilterSpecificationAttributeService.GetAjaxFilterSpecificationAttributeBySpecificationAttributeId(option.SpecificationAttributeId);
                        if (ajaxFilterSpecificationAttribute != null)
                        {
                            attributeFilter = new SpecificationAttributesModel
                            {
                                Id = attribute.Id,
                                Name = attribute.Name,
                                MaxDisplayForSpecifiation = ajaxFilterSpecificationAttribute.MaxSpecificationAttributesToDisplay,
                                CloseSpecificationAttributeByDefault = ajaxFilterSpecificationAttribute.CloseSpecificationAttributeByDefault,
                                AlternateName = ajaxFilterSpecificationAttribute.AlternateName,
                                DisplayOrder = ajaxFilterSpecificationAttribute.DisplayOrder,
                                HideProductCount = ajaxFilterSpecificationAttribute.HideProductCount,
                            };
                            model.SpecificationAttributes.Add(attributeFilter);
                        }
                    }

                    if (attributeFilter != null)
                    {
                        var optionDisplayOrder = 0;

                        attributeFilter.SpecificationAttributeOptions.Add(new SpecificationAttributeOptionsModel
                        {
                            Id = option.Id,
                            Name = option.Name,
                            CheckedState = selectedOptions?.Any(optionId => optionId == option.Id) == true ? CheckedState.Checked : CheckedState.UnChecked,
                            ColorSquaresRgb = option.Color,
                            Count = specificationResult.FirstOrDefault(x => x.Id == option.Id).Count,
                            DisplayOrder = optionDisplayOrder
                        });
                    }
                }
                model.SpecificationAttributes = model.SpecificationAttributes.OrderBy(x => x.DisplayOrder).ToList();
            }

            return model;
        }

        protected virtual async Task<FilterProductsModel> PrepareProductFilterModel(bool taxExempt, bool freeShipping, bool newProduct, bool discountedProduct)
        {
            var model = new FilterProductsModel();
            model.FreeShipping = freeShipping;
            model.NewProduct = newProduct;
            model.DiscountedProduct = discountedProduct;
            model.TaxExempt = taxExempt;

            return await Task.FromResult(model);
        }
        #endregion
    }
}
