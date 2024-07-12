using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.UI.Paging;
using NopStation.Plugin.Misc.AlgoliaSearch.Services;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Models
{
    public record AlgoliaPagingFilteringModel : BasePageableModel
    {
        #region Ctor

        public AlgoliaPagingFilteringModel()
        {
            AvailableSortOptions = new List<SelectListItem>();
            AvailableViewModes = new List<SelectListItem>();
            PageSizeOptions = new List<SelectListItem>();
            CategoryFilter = new CategoryFilterModel();
            ManufacturerFilter = new ManufacturerFilterModel();
            VendorFilter = new VendorFilterModel();
            RatingFilter = new RatingFilterModel();
            SpecificationFilter = new SpecificationFilterModel();
            //AttributeFilter = new AttributeFilterModel();
            PriceRangeFilter = new PriceRangeFilterModel();
        }

        #endregion

        #region Properties

        public bool AllowProductSorting { get; set; }
        public IList<SelectListItem> AvailableSortOptions { get; set; }

        public bool AllowProductViewModeChanging { get; set; }
        public IList<SelectListItem> AvailableViewModes { get; set; }

        public bool AllowCustomersToSelectPageSize { get; set; }
        public IList<SelectListItem> PageSizeOptions { get; set; }

        public CategoryFilterModel CategoryFilter { get; set; }

        public ManufacturerFilterModel ManufacturerFilter { get; set; }

        public VendorFilterModel VendorFilter { get; set; }

        public RatingFilterModel RatingFilter { get; set; }

        public SpecificationFilterModel SpecificationFilter { get; set; }

        //public AttributeFilterModel AttributeFilter { get; set; }

        public PriceRangeFilterModel PriceRangeFilter { get; set; }

        public int? OrderBy { get; set; }

        public string ViewMode { get; set; }

        public bool ShowProductsCount { get; set; }

        #endregion

        #region Nested class

        #region Category

        public class CategoryFilterModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "cid";

            #endregion 

            #region Utilities

            protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
            {
                //comma separated list of parameters to exclude
                const string excludedQueryStringParams = "pagenumber";
                var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var exclude in excludedQueryStringParamsSplitted)
                    url = webHelper.RemoveQueryString(url, exclude);
                return url;
            }

            #endregion

            public CategoryFilterModel()
            {
                Items = new List<SelectListItemDetails>();
            }

            public bool Enabled { get; set; }

            public List<SelectListItemDetails> Items { get; set; }

            public virtual IList<int> GetAlreadyFilteredCategoryIds(IWebHelper webHelper)
            {
                var result = new List<int>();

                var categoryIdsStr = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (string.IsNullOrEmpty(categoryIdsStr))
                    return result;

                foreach (var spec in categoryIdsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int.TryParse(spec.Trim(), out var cid);
                    if (!result.Contains(cid) && cid > 0)
                        result.Add(cid);
                }
                return result;
            }

            public virtual async Task PrepareCategoriesFiltersAsync(IList<int> alreadyFilteredCatIds, IList<FilterItemModel> filterableCatIds,
                ICategoryService categoryService, ILocalizationService localizationService, IWebHelper webHelper, IWorkContext workContext)
            {
                Enabled = false;

                alreadyFilteredCatIds = alreadyFilteredCatIds ?? new List<int>();
                var allOptions = await categoryService.GetCategoriesByIdsAsync(filterableCatIds.Select(x => x.Id).ToArray());

                if (!allOptions.Any())
                    return;

                //prepare the model properties
                Enabled = true;


                //get not filtered specification options
                Items = await allOptions.SelectAwait(async x =>
                {
                    //filter URL
                    var paramIds = alreadyFilteredCatIds.Contains(x.Id) ? alreadyFilteredCatIds.Except(new List<int> { x.Id }) :
                        alreadyFilteredCatIds.Concat(new List<int> { x.Id });

                    var filterUrl = !paramIds.Any() ? webHelper.RemoveQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM) :
                        webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM,
                        paramIds.OrderBy(id => id).Select(id => id.ToString()).ToArray());

                    return new SelectListItemDetails()
                    {
                        Id = x.Id,
                        FilterUrl = ExcludeQueryStringParams(filterUrl, webHelper),
                        Count = filterableCatIds.FirstOrDefault(o => o.Id == x.Id).Count,
                        Selected = alreadyFilteredCatIds.Contains(x.Id),
                        Text = await localizationService.GetLocalizedAsync(x, y => y.Name),
                    };
                }).ToListAsync();
            }
        }

        #endregion

        #region Manufacturer

        public class ManufacturerFilterModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "mid";

            #endregion 

            public ManufacturerFilterModel()
            {
                Items = new List<SelectListItemDetails>();
            }

            #region Utilities

            protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
            {
                //comma separated list of parameters to exclude
                const string excludedQueryStringParams = "pagenumber";
                var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var exclude in excludedQueryStringParamsSplitted)
                    url = webHelper.RemoveQueryString(url, exclude);
                return url;
            }

            #endregion

            public bool Enabled { get; set; }

            public List<SelectListItemDetails> Items { get; set; }

            public virtual IList<int> GetAlreadyFilteredManufacturerIds(IWebHelper webHelper)
            {
                var result = new List<int>();

                var manufacturerIdsStr = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (string.IsNullOrEmpty(manufacturerIdsStr))
                    return result;

                foreach (var spec in manufacturerIdsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int.TryParse(spec.Trim(), out var mid);
                    if (!result.Contains(mid) && mid > 0)
                        result.Add(mid);
                }
                return result;
            }

            public virtual async Task PrepareManufacsFiltersAsync(IList<int> alreadyFilteredManfIds, IList<FilterItemModel> filterableManfIds,
                IAlgoliaCatalogService catalogService, ILocalizationService localizationService, IWebHelper webHelper, IWorkContext workContext)
            {
                Enabled = false;

                alreadyFilteredManfIds = alreadyFilteredManfIds ?? new List<int>();
                var allOptions = catalogService.GetManufacturersByIds(filterableManfIds.Select(x => x.Id).ToArray());

                if (!allOptions.Any())
                    return;

                //prepare the model properties
                Enabled = true;

                //get not filtered specification options
                Items = await allOptions.SelectAwait(async x =>
                {
                    //filter URL
                    var paramIds = alreadyFilteredManfIds.Contains(x.Id) ? alreadyFilteredManfIds.Except(new List<int> { x.Id }) :
                        alreadyFilteredManfIds.Concat(new List<int> { x.Id });

                    var filterUrl = !paramIds.Any() ? webHelper.RemoveQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM) :
                        webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM,
                        paramIds.OrderBy(id => id).Select(id => id.ToString()).ToArray());

                    return new SelectListItemDetails()
                    {
                        Id = x.Id,
                        FilterUrl = ExcludeQueryStringParams(filterUrl, webHelper),
                        Count = filterableManfIds.FirstOrDefault(o => o.Id == x.Id).Count,
                        Selected = alreadyFilteredManfIds.Contains(x.Id),
                        Text = await localizationService.GetLocalizedAsync(x, y => y.Name),
                    };
                }).ToListAsync();
            }
        }

        #endregion

        #region Vendor

        public class VendorFilterModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "vid";

            #endregion 

            public VendorFilterModel()
            {
                Items = new List<SelectListItemDetails>();
            }

            #region Utilities

            protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
            {
                //comma separated list of parameters to exclude
                const string excludedQueryStringParams = "pagenumber";
                var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var exclude in excludedQueryStringParamsSplitted)
                    url = webHelper.RemoveQueryString(url, exclude);
                return url;
            }

            #endregion

            public bool Enabled { get; set; }

            public List<SelectListItemDetails> Items { get; set; }

            public virtual IList<int> GetAlreadyFilteredVendorIds(IWebHelper webHelper)
            {
                var result = new List<int>();

                var vendorIdsStr = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (string.IsNullOrEmpty(vendorIdsStr))
                    return result;

                foreach (var spec in vendorIdsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int.TryParse(spec.Trim(), out var vid);
                    if (!result.Contains(vid) && vid > 0)
                        result.Add(vid);
                }
                return result;
            }

            public virtual async Task PrepareVendorsFiltersAsync(IList<int> alreadyFilteredVendorIds, IList<FilterItemModel> filterableVendorIds,
                IVendorService vendorService, ILocalizationService localizationService, IWebHelper webHelper, IWorkContext workContext)
            {
                Enabled = false;

                alreadyFilteredVendorIds = alreadyFilteredVendorIds ?? new List<int>();
                var allOptions = new List<Vendor>();
                foreach (var item in filterableVendorIds)
                {
                    var vendor = await vendorService.GetVendorByIdAsync(item.Id);
                    if (vendor == null || vendor.Deleted || !vendor.Active)
                        continue;

                    allOptions.Add(vendor);
                }

                if (!allOptions.Any())
                    return;

                //prepare the model properties
                Enabled = true;

                //get not filtered specification options
                Items = allOptions.Select(x =>
                {
                    //filter URL
                    var paramIds = alreadyFilteredVendorIds.Contains(x.Id) ? alreadyFilteredVendorIds.Except(new List<int> { x.Id }) :
                        alreadyFilteredVendorIds.Concat(new List<int> { x.Id });

                    var filterUrl = !paramIds.Any() ? webHelper.RemoveQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM) :
                        webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM,
                        paramIds.OrderBy(id => id).Select(id => id.ToString()).ToArray());

                    return new SelectListItemDetails()
                    {
                        Id = x.Id,
                        FilterUrl = ExcludeQueryStringParams(filterUrl, webHelper),
                        Count = filterableVendorIds.FirstOrDefault(o => o.Id == x.Id).Count,
                        Selected = alreadyFilteredVendorIds.Contains(x.Id),
                        Text = localizationService.GetLocalizedAsync(x, y => y.Name).Result,
                    };
                }).ToList();
            }
        }

        #endregion

        #region Specification

        public record SpecificationFilterModel : BaseNopModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "sid";

            #endregion

            #region Ctor

            public SpecificationFilterModel()
            {
                Items = new List<SpecificationFilterItem>();
            }

            #endregion

            #region Utilities

            protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
            {
                //comma separated list of parameters to exclude
                const string excludedQueryStringParams = "pagenumber";
                var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var exclude in excludedQueryStringParamsSplitted)
                    url = webHelper.RemoveQueryString(url, exclude);
                return url;
            }

            #endregion

            #region Methods

            public virtual List<FilteredGroupModel> GetAlreadyFilteredSpecOptionIds(IWebHelper webHelper)
            {
                var result = new List<FilteredGroupModel>();

                var alreadyFilteredSpecsStr = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (string.IsNullOrWhiteSpace(alreadyFilteredSpecsStr))
                    return result;

                foreach (var spec in alreadyFilteredSpecsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var token = spec.Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (token.Length == 2 && int.TryParse(token[0], out int optionId) &&
                        int.TryParse(token[1], out int specId) && !result.Any(x => x.OptionId == optionId))
                        result.Add(new FilteredGroupModel() { OptionId = optionId, Id = specId });
                }
                return result;
            }

            public virtual async Task PrepareSpecsFiltersAsync(IList<FilteredGroupModel> alreadyFilteredSpecOptionIds, IList<FilterItemModel> filterableSpecOptionIds,
                ISpecificationAttributeService specificationAttributeService, ILocalizationService localizationService, IWebHelper webHelper,
                IWorkContext workContext, IStaticCacheManager cacheManager)
            {
                Enabled = false;
                var optionIds = filterableSpecOptionIds != null
                    ? string.Join(",", filterableSpecOptionIds) : string.Empty;
                //var cacheKey = cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.SpecsFilterModelKey, optionIds, workContext.WorkingLanguage.Id);

                //var cacheKey = string.Format(NopModelCacheDefaults.SpecsFilterModelKey, optionIds, workContext.WorkingLanguage.Id);

                var allOptions = await specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(filterableSpecOptionIds.Select(x => x.Id).ToArray());


                var allFilters = await allOptions.SelectAwait(async sao =>
                {
                    var specAttribute = await specificationAttributeService.GetSpecificationAttributeByIdAsync(sao.SpecificationAttributeId);

                    return new SpecificationAttributeOptionFilter
                    {
                        SpecificationAttributeId = specAttribute.Id,
                        SpecificationAttributeName = await localizationService.GetLocalizedAsync(specAttribute, x => x.Name, (await workContext.GetWorkingLanguageAsync()).Id),
                        SpecificationAttributeDisplayOrder = specAttribute.DisplayOrder,
                        SpecificationAttributeOptionId = sao.Id,
                        SpecificationAttributeOptionName = await localizationService.GetLocalizedAsync(sao, x => x.Name, (await workContext.GetWorkingLanguageAsync()).Id),
                        SpecificationAttributeOptionColorRgb = sao.ColorSquaresRgb,
                        SpecificationAttributeOptionDisplayOrder = sao.DisplayOrder
                    };
                }).ToListAsync();

                if (!allFilters.Any())
                    return;

                //prepare the model properties
                Enabled = true;

                //get not filtered specification options
                Items = allFilters.Select(x =>
                {
                    var selected = false;
                    //filter URL
                    var paramIds = new List<FilteredGroupModel>();
                    if (alreadyFilteredSpecOptionIds.Any(y => y.OptionId == x.SpecificationAttributeOptionId))
                    {
                        selected = true;
                        paramIds = alreadyFilteredSpecOptionIds.Where(y => y.OptionId != x.SpecificationAttributeOptionId).ToList();
                    }
                    else
                    {
                        paramIds = alreadyFilteredSpecOptionIds.ToList();
                        paramIds.Add(new FilteredGroupModel() { Id = x.SpecificationAttributeId, OptionId = x.SpecificationAttributeOptionId });
                    }

                    var filterUrl = !paramIds.Any() ? webHelper.RemoveQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM) :
                        webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM,
                        paramIds.Select(y => y.OptionId + "-" + y.Id).ToArray());

                    return new SpecificationFilterItem()
                    {
                        Id = x.SpecificationAttributeOptionId,
                        SpecificationAttributeName = x.SpecificationAttributeName,
                        SpecificationAttributeOptionName = x.SpecificationAttributeOptionName,
                        SpecificationAttributeOptionColorRgb = x.SpecificationAttributeOptionColorRgb,
                        FilterUrl = ExcludeQueryStringParams(filterUrl, webHelper),
                        Selected = selected,
                        Count = filterableSpecOptionIds.FirstOrDefault(o => o.Id == x.SpecificationAttributeOptionId).Count
                    };
                }).ToList();
            }

            #endregion

            #region Properties

            public bool Enabled { get; set; }

            public IList<SpecificationFilterItem> Items { get; set; }

            #endregion
        }

        public record SpecificationFilterItem : BaseNopEntityModel
        {
            public string SpecificationAttributeName { get; set; }

            public string SpecificationAttributeOptionName { get; set; }

            public string SpecificationAttributeOptionColorRgb { get; set; }

            public string FilterUrl { get; set; }

            public bool Selected { get; set; }

            public int Count { get; set; }
        }

        #endregion

        #region Attribute

        //public class AttributeFilterModel
        //{
        //    #region Const

        //    private const string QUERYSTRINGPARAM = "attrs";

        //    #endregion

        //    #region Ctor

        //    public AttributeFilterModel()
        //    {
        //        Items = new List<AttributeFilterItem>();
        //    }

        //    #endregion

        //    #region Utilities

        //    protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
        //    {
        //        //comma separated list of parameters to exclude
        //        const string excludedQueryStringParams = "pagenumber";
        //        var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var exclude in excludedQueryStringParamsSplitted)
        //            url = webHelper.RemoveQueryString(url, exclude);
        //        return url;
        //    }

        //    #endregion

        //    #region Methods

        //    public virtual List<FilteredGroupModel> GetAlreadyFilteredAttrValueIds(IWebHelper webHelper)
        //    {
        //        var result = new List<FilteredGroupModel>();

        //        var alreadyFilteredSpecsStr = webHelper.QueryString<string>(QUERYSTRINGPARAM);
        //        if (string.IsNullOrWhiteSpace(alreadyFilteredSpecsStr))
        //            return result;

        //        foreach (var spec in alreadyFilteredSpecsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            var token = spec.Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //            if (token.Length == 2 && int.TryParse(token[0], out int optionId) &&
        //                int.TryParse(token[1], out int specId) && !result.Any(x => x.OptionId == optionId))
        //                result.Add(new FilteredGroupModel() { OptionId = optionId, Id = specId });
        //        }
        //        return result;
        //    }

        //    public virtual void PrepareAttrsFilters(IList<FilteredGroupModel> alreadyFilteredAttrIds, IList<FilterItemModel> filterableAttrIds,
        //        IAlgoliaCatalogService catalogService, ILocalizationService localizationService, IWebHelper webHelper,
        //        IWorkContext workContext, ICacheManager cacheManager)
        //    {
        //        Enabled = false;

        //        var allOptions = catalogService.GetAttributeValuesByIds(filterableAttrIds.Select(x => x.Id).ToArray());

        //        if (!allOptions.Any())
        //            return;

        //        //prepare the model properties
        //        Enabled = true;

        //        //get not filtered specification options
        //        Items = allOptions.Select(x =>
        //        {
        //            var selected = false;
        //            //filter URL
        //            var paramIds = new List<FilteredGroupModel>();
        //            if (alreadyFilteredAttrIds.Any(y => y.OptionId == x.Id))
        //            {
        //                selected = true;
        //                paramIds = alreadyFilteredAttrIds.Where(y => y.OptionId != x.Id
        //                ).ToList();
        //            }
        //            else
        //            {
        //                paramIds = alreadyFilteredAttrIds.ToList();
        //                paramIds.Add(new FilteredGroupModel() { Id = x.ProductAttributeMapping.ProductAttributeId, OptionId = x.Id });
        //            }

        //            var filterUrl = !paramIds.Any() ? webHelper.RemoveQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM) :
        //                webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM,
        //                paramIds.Select(y => y.OptionId + "-" + y.Id).ToArray());

        //            return new AttributeFilterItem()
        //            {
        //                Id = x.Id,
        //                AttributeName = localizationService.GetLocalized(x.ProductAttributeMapping.ProductAttribute, y => y.Name),
        //                AttributeValueName = localizationService.GetLocalized(x, y => y.Name),
        //                AttributeValueColorRgb = x.ColorSquaresRgb,
        //                FilterUrl = ExcludeQueryStringParams(filterUrl, webHelper),
        //                Selected = selected,
        //                Count = filterableAttrIds.FirstOrDefault(o => o.Id == x.Id).Count
        //            };
        //        }).ToList();
        //    }

        //    #endregion

        //    #region Properties

        //    public bool Enabled { get; set; }

        //    public IList<AttributeFilterItem> Items { get; set; }

        //    #endregion
        //}

        //public class AttributeFilterItem : BaseNopEntityModel
        //{
        //    public string AttributeName { get; set; }

        //    public string AttributeValueName { get; set; }

        //    public string AttributeValueColorRgb { get; set; }

        //    public string FilterUrl { get; set; }

        //    public int Count { get; set; }

        //    public bool Selected { get; set; }
        //}

        #endregion

        #region Rating

        public class RatingFilterModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "rating";

            #endregion 

            public RatingFilterModel()
            {
                Items = new List<SelectListItemDetails>();
            }

            public bool Enabled { get; set; }

            public List<SelectListItemDetails> Items { get; set; }

            public virtual IList<int> GetAlreadyFilteredRatingIds(IWebHelper webHelper)
            {
                var result = new List<int>();

                var ratingIdsStr = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (string.IsNullOrEmpty(ratingIdsStr))
                    return result;

                foreach (var spec in ratingIdsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int.TryParse(spec.Trim(), out int specId);
                    if (!result.Contains(specId))
                        result.Add(specId);
                }
                return result;
            }

            public virtual async Task PrepareRatingsFiltersAsync(IList<int> alreadyFilteredRatingIds, IList<FilterItemModel> filterableRatingIds,
                ILocalizationService localizationService, IWebHelper webHelper, IWorkContext workContext)
            {
                Enabled = false;

                alreadyFilteredRatingIds = alreadyFilteredRatingIds ?? new List<int>();

                if (!filterableRatingIds.Any())
                    return;

                //prepare the model properties
                Enabled = true;

                //get not filtered specification options
                for (int i = 1; i <= 5; i++)
                {
                    var star = filterableRatingIds.FirstOrDefault(x => x.Id == i);
                    var paramIds = alreadyFilteredRatingIds.Contains(i) ? alreadyFilteredRatingIds.Except(new List<int> { i }) :
                        alreadyFilteredRatingIds.Concat(new List<int> { i });

                    var filterUrl = !paramIds.Any() ? webHelper.RemoveQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM) :
                        webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM,
                        paramIds.OrderBy(id => id).Select(id => id.ToString()).ToArray());

                    Items.Add(
                        new SelectListItemDetails()
                        {
                            Count = star == null ? 0 : star.Count,
                            Id = i,
                            Selected = alreadyFilteredRatingIds.Contains(i),
                            FilterUrl = filterUrl,
                            Text = await localizationService.GetResourceAsync(AlgoliaDefaults.RatingResourceKey[i - 1])
                        });
                }
            }
        }

        #endregion

        #region Price range

        public class PriceRangeFilterModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "price";

            #endregion 

            public bool Enabled { get; set; }

            public decimal MinPrice { get; set; }

            public string MinPriceStr { get; set; }

            public decimal MaxPrice { get; set; }

            public string MaxPriceStr { get; set; }

            public decimal CurrentMinPrice { get; set; }

            public decimal CurrentMaxPrice { get; set; }

            public string NonFiteredUrl { get; set; }

            public virtual PriceRange GetSelectedPriceRange(IWebHelper webHelper)
            {
                var range = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (string.IsNullOrEmpty(range))
                    return null;
                var fromTo = range.Trim().Split(new[] { '-' });
                if (fromTo.Length == 2)
                {
                    decimal? from = null;
                    if (!string.IsNullOrEmpty(fromTo[0]) && !string.IsNullOrEmpty(fromTo[0].Trim()))
                        from = decimal.Parse(fromTo[0].Trim(), new CultureInfo("en-US"));
                    decimal? to = null;
                    if (!string.IsNullOrEmpty(fromTo[1]) && !string.IsNullOrEmpty(fromTo[1].Trim()))
                        to = decimal.Parse(fromTo[1].Trim(), new CultureInfo("en-US"));
                    return new PriceRange() { From = from, To = to };
                }
                return null;
            }

            public async Task PreparePriceRangeFiltersAsync(PriceRange priceRange, decimal minPrice, decimal maxPrice,
                ICurrencyService currencyService, IWebHelper webHelper, IWorkContext workContext, IPriceFormatter priceFormatter)
            {
                Enabled = false;

                if (maxPrice == minPrice)
                    return;

                Enabled = true;

                minPrice = await currencyService.ConvertFromPrimaryStoreCurrencyAsync(minPrice, await workContext.GetWorkingCurrencyAsync());
                maxPrice = await currencyService.ConvertFromPrimaryStoreCurrencyAsync(maxPrice, await workContext.GetWorkingCurrencyAsync());

                var url = webHelper.RemoveQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM);
                NonFiteredUrl = webHelper.RemoveQueryString(url, "pagenumber");

                MinPrice = minPrice;
                MaxPrice = maxPrice;
                MinPriceStr = await priceFormatter.FormatPriceAsync(minPrice);
                MaxPriceStr = await priceFormatter.FormatPriceAsync(maxPrice);

                var currentMaxPrice = priceRange?.To;
                var currentMinPrice = priceRange?.From;

                if (currentMaxPrice.HasValue)
                {
                    currentMaxPrice = currentMaxPrice > MaxPrice ? MaxPrice : currentMaxPrice.Value;
                    currentMaxPrice = currentMaxPrice < MinPrice ? MinPrice : currentMaxPrice.Value;
                }
                else
                    currentMaxPrice = MaxPrice;

                if (currentMinPrice.HasValue)
                {
                    currentMinPrice = currentMinPrice < MinPrice ? MinPrice : currentMinPrice.Value;
                    currentMinPrice = currentMinPrice > MaxPrice ? MaxPrice : currentMinPrice.Value;
                }
                else
                    currentMinPrice = MinPrice;

                CurrentMaxPrice = decimal.Round(currentMaxPrice.Value, 2, MidpointRounding.AwayFromZero);
                CurrentMinPrice = decimal.Round(currentMinPrice.Value, 2, MidpointRounding.AwayFromZero);
            }
        }

        #endregion

        public record SelectListItemDetails : BaseNopEntityModel
        {
            public int Count { get; set; }

            public string GroupName { get; set; }

            public bool Selected { get; set; }

            public string Text { get; set; }

            public string FilterUrl { get; set; }
        }

        #endregion
    }
}
