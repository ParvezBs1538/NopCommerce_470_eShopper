using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Services;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Factories
{
    public class AlgoliaCatalogModelFactory : IAlgoliaCatalogModelFactory
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly AlgoliaSearchSettings _algoliaSearchSettings;
        private readonly IAlgoliaHelperFactory _algoliaHelperFactory;
        private readonly IAlgoliaCatalogService _algoliaCatalogService;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Ctor

        public AlgoliaCatalogModelFactory(IWebHelper webHelper,
            ISpecificationAttributeService specificationAttributeService,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IStaticCacheManager cacheManager,
            IVendorService vendorService,
            IWorkContext workContext,
            AlgoliaSearchSettings algoliaSearchSettings,
            IAlgoliaHelperFactory algoliaHelperFactory,
            IAlgoliaCatalogService algoliaCatalogService,
            IPriceFormatter priceFormatter)
        {
            _webHelper = webHelper;
            _specificationAttributeService = specificationAttributeService;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _cacheManager = cacheManager;
            _vendorService = vendorService;
            _workContext = workContext;
            _algoliaSearchSettings = algoliaSearchSettings;
            _algoliaHelperFactory = algoliaHelperFactory;
            _algoliaCatalogService = algoliaCatalogService;
            _priceFormatter = priceFormatter;
        }

        #endregion

        #region Common

        public virtual async Task PrepareSortingOptionsAsyync(AlgoliaPagingFilteringModel pagingFilteringModel, AlgoliaPagingFilteringModel command)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.OrderBy = command.OrderBy.HasValue && Enum.IsDefined(typeof(AlgoliaSortingEnum), command.OrderBy) ? command.OrderBy : 0;
            //set the order by position by default
            pagingFilteringModel.OrderBy = command.OrderBy;
            command.OrderBy = (int)ProductSortingEnum.Position;

            //ensure that product sorting is enabled
            if (!_algoliaSearchSettings.AllowProductSorting)
                return;

            //order sorting options
            var sortingOptions = _algoliaSearchSettings.AllowedSortingOptions;
            sortingOptions.Insert(0, 0);

            pagingFilteringModel.AllowProductSorting = true;
            command.OrderBy = pagingFilteringModel.OrderBy ?? sortingOptions.FirstOrDefault();

            //prepare available model sorting options
            var currentPageUrl = _webHelper.GetThisPageUrl(true);
            foreach (var option in sortingOptions)
            {
                pagingFilteringModel.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync((AlgoliaSortingEnum)option),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "orderby", option.ToString()),
                    Selected = option == command.OrderBy
                });
            }
        }

        public virtual async Task PrepareViewModesAsync(AlgoliaPagingFilteringModel pagingFilteringModel, AlgoliaPagingFilteringModel command)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            pagingFilteringModel.AllowProductViewModeChanging = _algoliaSearchSettings.AllowProductViewModeChanging;

            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                ? command.ViewMode
                : _algoliaSearchSettings.DefaultViewMode;
            pagingFilteringModel.ViewMode = viewMode;
            if (pagingFilteringModel.AllowProductViewModeChanging)
            {
                var currentPageUrl = _webHelper.GetThisPageUrl(true);
                //grid
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.Grid"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode", "grid"),
                    Selected = viewMode == "grid"
                });
                //list
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.List"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode", "list"),
                    Selected = viewMode == "list"
                });
            }
        }

        public virtual void PreparePageSizeOptions(AlgoliaPagingFilteringModel pagingFilteringModel, AlgoliaPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.PageNumber <= 0)
            {
                command.PageNumber = 1;
            }
            pagingFilteringModel.AllowCustomersToSelectPageSize = false;
            if (allowCustomersToSelectPageSize && pageSizeOptions != null)
            {
                var pageSizes = pageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        if (int.TryParse(pageSizes.FirstOrDefault(), out int temp))
                        {
                            if (temp > 0)
                            {
                                command.PageSize = temp;
                            }
                        }
                    }

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.RemoveQueryString(currentPageUrl, "pagenumber");

                    foreach (var pageSize in pageSizes)
                    {
                        if (!int.TryParse(pageSize, out int temp))
                        {
                            continue;
                        }
                        if (temp <= 0)
                        {
                            continue;
                        }

                        pagingFilteringModel.PageSizeOptions.Add(new SelectListItem
                        {
                            Text = pageSize,
                            Value = _webHelper.ModifyQueryString(sortUrl, "pagesize", pageSize),
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (pagingFilteringModel.PageSizeOptions.Any())
                    {
                        pagingFilteringModel.PageSizeOptions = pagingFilteringModel.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
                        pagingFilteringModel.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                        {
                            command.PageSize = int.Parse(pagingFilteringModel.PageSizeOptions.First().Text);
                        }
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = fixedPageSize;
            }

            //ensure pge size is specified
            if (command.PageSize <= 0)
            {
                command.PageSize = fixedPageSize;
            }
        }

        protected void PrepareFilterOptions(AlgoliaPagingFilteringModel pagingFilteringContext)
        {
            pagingFilteringContext.AllowProductSorting = _algoliaSearchSettings.AllowProductSorting;
            pagingFilteringContext.AllowCustomersToSelectPageSize = _algoliaSearchSettings.AllowCustomersToSelectPageSize;
            pagingFilteringContext.AllowProductViewModeChanging = _algoliaSearchSettings.AllowProductViewModeChanging;
            pagingFilteringContext.ShowProductsCount = _algoliaSearchSettings.ShowProductsCount;
        }

        #endregion

        #region Methods

        public async Task<SearchModel> PrepareSearchModelAsync(SearchModel model, AlgoliaPagingFilteringModel command)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var searchTerms = model.q;
            if (searchTerms == null)
                searchTerms = "";
            searchTerms = searchTerms.Trim();

            //sorting
            await PrepareSortingOptionsAsyync(model.PagingFilteringContext, command);
            //view mode
            await PrepareViewModesAsync(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                _algoliaSearchSettings.AllowCustomersToSelectPageSize,
                _algoliaSearchSettings.SearchPagePageSizeOptions,
                _algoliaSearchSettings.SearchPageProductsPerPage);
            PrepareFilterOptions(model.PagingFilteringContext);

            var availableFilters = await _algoliaHelperFactory.GetAlgoliaFiltersAsync(model.q);

            var selectedCatIds = model.PagingFilteringContext.CategoryFilter.GetAlreadyFilteredCategoryIds(_webHelper);
            var selectedManfIds = model.PagingFilteringContext.ManufacturerFilter.GetAlreadyFilteredManufacturerIds(_webHelper);
            var selectedVendIds = model.PagingFilteringContext.VendorFilter.GetAlreadyFilteredVendorIds(_webHelper);
            var selectedSpecIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            var selectedRatings = model.PagingFilteringContext.RatingFilter.GetAlreadyFilteredRatingIds(_webHelper);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper);

            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(selectedPriceRange.From.Value, await _workContext.GetWorkingCurrencyAsync());

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(selectedPriceRange.To.Value, await _workContext.GetWorkingCurrencyAsync());
            }
            var products = await _algoliaHelperFactory.SearchProductsAsync(
                searchTerms: searchTerms,
                cids: selectedCatIds,
                mids: selectedManfIds,
                vids: selectedVendIds,
                specids: selectedSpecIds,
                ratings: selectedRatings,
                maxPrice: maxPriceConverted,
                minPrice: minPriceConverted,
                orderby: command.OrderBy,
                pageIndex: command.PageIndex,
                pageSize: command.PageSize);

            model.Products = products;
            model.PagingFilteringContext.LoadPagedList(products);
            await model.PagingFilteringContext.CategoryFilter.PrepareCategoriesFiltersAsync(selectedCatIds, availableFilters.AvailableCategories,
                _categoryService, _localizationService, _webHelper, _workContext);

            await model.PagingFilteringContext.ManufacturerFilter.PrepareManufacsFiltersAsync(selectedManfIds, availableFilters.AvailableManufacturers,
                _algoliaCatalogService, _localizationService, _webHelper, _workContext);

            await model.PagingFilteringContext.VendorFilter.PrepareVendorsFiltersAsync(selectedVendIds, availableFilters.AvailableVendors,
                _vendorService, _localizationService, _webHelper, _workContext);

            await model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFiltersAsync(selectedSpecIds, availableFilters.AvailableSpecifications,
                 _specificationAttributeService, _localizationService, _webHelper, _workContext, _cacheManager);

            await model.PagingFilteringContext.RatingFilter.PrepareRatingsFiltersAsync(selectedRatings, availableFilters.AvailableRatings,
                _localizationService, _webHelper, _workContext);

            await model.PagingFilteringContext.PriceRangeFilter.PreparePriceRangeFiltersAsync(selectedPriceRange, availableFilters.MinPrice,
                availableFilters.MaxPrice, _currencyService, _webHelper, _workContext, _priceFormatter);

            return model;
        }

        #endregion
    }
}
