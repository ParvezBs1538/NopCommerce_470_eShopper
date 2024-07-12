using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Algolia.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Services;
using static NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models.AlgoliaOverviewModel;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Factories
{
    public class AlgoliaHelperFactory : IAlgoliaHelperFactory
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly CatalogSettings _catalogSettings;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ProductUploadHub _productUploadHub;
        private readonly IAlgoliaCatalogService _algoliaCatalogService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly AlgoliaSearchSettings _algoliaSearchSettings;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly IVendorService _vendorService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IAlgoliaUpdatableItemService _algoliaUpdatableItemService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly IProductTagService _productTagService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INopDataProvider _dataProvider;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public AlgoliaHelperFactory(ILogger logger,
            CatalogSettings catalogSettings,
            IStoreMappingService storeMappingService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ProductUploadHub productUploadHub,
            IAlgoliaCatalogService algoliaCatalogService,
            IProductModelFactory productModelFactory,
            AlgoliaSearchSettings algoliaSearchSettings,
            IProductService productService,
            IStaticCacheManager staticCacheManager,
            IWebHelper webHelper,
            IWorkContext workContext,
            IPictureService pictureService,
            IVendorService vendorService,
            IUrlRecordService urlRecordService,
            IAlgoliaUpdatableItemService algoliaUpdatableItemService,
            ISpecificationAttributeService specificationAttributeService,
            IProductAttributeService productAttributeService,
            ICurrencyService currencyService,
            IProductTagService productTagService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            INopDataProvider dataProvider,
            ILanguageService languageService)
        {
            _logger = logger;
            _catalogSettings = catalogSettings;
            _storeMappingService = storeMappingService;
            _settingService = settingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _productUploadHub = productUploadHub;
            _algoliaCatalogService = algoliaCatalogService;
            _productModelFactory = productModelFactory;
            _algoliaSearchSettings = algoliaSearchSettings;
            _productService = productService;
            _cacheManager = staticCacheManager;
            _webHelper = webHelper;
            _workContext = workContext;
            _pictureService = pictureService;
            _vendorService = vendorService;
            _urlRecordService = urlRecordService;
            _algoliaUpdatableItemService = algoliaUpdatableItemService;
            _specificationAttributeService = specificationAttributeService;
            _productAttributeService = productAttributeService;
            _currencyService = currencyService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _dataProvider = dataProvider;
            _languageService = languageService;
        }

        #endregion

        #region Utilities

        protected Algolia.Search.Index GetDefaultIndex(AlgoliaClient client, ConfigurationModel model, out dynamic settings)
        {
            var allIndices = client.ListIndexes();
            settings = new JObject();

            var searchableAttributes = AlgoliaDefaults.SearchableAttributes;
            if (_algoliaSearchSettings.EnableMultilingualSearch)
            {
                var languages = _languageService.GetAllLanguages();
                foreach (var language in languages)
                {
                    searchableAttributes = searchableAttributes.Append(string.Format(AlgoliaDefaults.MultilingualProductNameFormate, language.UniqueSeoCode)).ToArray();
                }
            }

            if (!allIndices["items"].Any(x => x["name"].ToString().Equals(AlgoliaDefaults.DefaultIndexName)))
            {
                var newIndex = client.InitIndex(AlgoliaDefaults.DefaultIndexName);


                settings.searchableAttributes = new JArray(searchableAttributes);
                settings.attributesForFaceting = new JArray(AlgoliaDefaults.FacetedAttributes);

                var setSettingsResponse = newIndex.SetSettings(settings, true);
                if (setSettingsResponse.taskID != null)
                    newIndex.WaitTask(setSettingsResponse.taskID.ToString());

                return newIndex;
            }

            var index = client.InitIndex(AlgoliaDefaults.DefaultIndexName);
            settings = (dynamic)index.GetSettings();
            if (model.UpdateIndicesModel.ResetSearchableAttributeSettings)
                settings.searchableAttributes = new JArray(searchableAttributes);
            if (model.UpdateIndicesModel.ResetFacetedAttributeSettings)
                settings.attributesForFaceting = new JArray(AlgoliaDefaults.FacetedAttributes);

            return index;
        }

        protected async Task<JObject> GetProductModelObjectAsync(Product product)
        {
            var productModel = await PrepareAlgoliaOverviewModelAsync(product);

            var dynamicObject = new ExpandoObject() as IDictionary<string, object>;
            var properties = productModel.Product.GetType().GetProperties();
            foreach (var property in properties.Where(x => x.CanRead))
                dynamicObject.Add(property.Name, property.GetValue(productModel.Product, null));

            dynamicObject.Add("objectID", productModel.Product.Id);
            dynamicObject.Add("AutoCompleteImageUrl", productModel.AutoCompleteImageUrl);
            dynamicObject.Add("Rating", productModel.Rating);
            dynamicObject.Add("Price", productModel.Price.ToString(new CultureInfo("en-US")));
            dynamicObject.Add("PriceValue", productModel.Price);
            dynamicObject.Add("OldPrice", productModel.OldPrice);
            dynamicObject.Add("FilterableCategories", productModel.FilterableCategories);
            dynamicObject.Add("FilterableManufacturers", productModel.FilterableManufacturers);
            dynamicObject.Add("FilterableSpecifications", productModel.FilterableSpecifications);
            dynamicObject.Add("FilterableAttributes", productModel.FilterableAttributes);
            dynamicObject.Add("FilterableVendor", productModel.FilterableVendor);
            dynamicObject.Add("FilterableKeywords", productModel.FilterableKeywords);
            dynamicObject.Add("ProductCombinations", productModel.ProductCombinations);
            dynamicObject.Add("CreatedOn", productModel.CreatedOn);
            dynamicObject.Add("LimitedToStores", product.LimitedToStores ? 1 : 0);
            dynamicObject.Add("GTIN", product.Gtin);

            if (_algoliaSearchSettings.EnableMultilingualSearch)
            {
                var languages = await _languageService.GetAllLanguagesAsync();
                foreach (var language in languages)
                {
                    var productLocalizeName = await _localizationService.GetLocalizedAsync(product, entity => entity.Name, language.Id, false, false);
                    if (!string.IsNullOrEmpty(productLocalizeName))
                    {
                        dynamicObject.Add(string.Format(AlgoliaDefaults.MultilingualProductNameFormate, language.UniqueSeoCode), productLocalizeName);
                    }
                }
            }

            var storeIds = Array.Empty<int>();
            if (!_catalogSettings.IgnoreStoreLimitations && product.LimitedToStores)
            {
                var stores = await _storeMappingService.GetStoreMappingsAsync(product);
                storeIds = stores.Select(x => x.StoreId).ToArray();
            }

            dynamicObject.Add("Stores", storeIds);

            var jobject = JObject.FromObject(dynamicObject);
            return jobject;
        }

        protected async Task<AlgoliaOverviewModel> PrepareAlgoliaOverviewModelAsync(Product product)
        {

            var productModel = (await _productModelFactory.PrepareProductOverviewModelsAsync(new List<Product>() { product })).FirstOrDefault();

            var model = new AlgoliaOverviewModel
            {
                Product = productModel,
                objectID = product.Id.ToString(),
                Price = product.Price,
                OldPrice = product.OldPrice,
                Rating = productModel.ReviewOverviewModel.TotalReviews < 1 ? 0 :
                productModel.ReviewOverviewModel.RatingSum / productModel.ReviewOverviewModel.TotalReviews,
                CreatedOn = product.CreatedOnUtc.Ticks,
                AutoCompleteImageUrl = await GetAutoCompleteImageUrlAsync(product),
            };

            var productTags = await _productTagService.GetAllProductTagsByProductIdAsync(product.Id);
            // One complete

            if (productTags.Any())
                model.FilterableKeywords = productTags.Select(x => x.Name).ToList();

            model.FilterableVendor = await PrepareVendorModelAsync(product);
            model.FilterableCategories = await PrepareCategoryListModelAsync(product);
            model.FilterableManufacturers = await PrepareManufacturerListModelAsync(product);
            model.FilterableSpecifications = await PrepareSpecificationListModelAsync(product);
            model.FilterableAttributes = await PrepareAttributeListModelAsync(product);
            model.ProductCombinations = await PrepareCombinationListModelAsync(product);

            return model;
        }

        protected async Task<IList<AlgoliaOverviewModel.AttributeModel>> PrepareAttributeListModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cache = new CacheKey(AlgoliaModelCacheDefaults.ProductAttrsModelKey, AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKey);

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, product.Id, (await _workContext.GetWorkingLanguageAsync()).Id);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new List<AlgoliaOverviewModel.AttributeModel>();
                var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                foreach (var productAttributeMapping in productAttributeMappings)
                {
                    var productAttributeValues = await _productAttributeService.GetProductAttributeValuesAsync(productAttributeMapping.Id);

                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);
                    foreach (var value in productAttributeValues)
                    {
                        var m = new AlgoliaOverviewModel.AttributeModel()
                        {
                            AttributeId = productAttributeMapping.ProductAttributeId,
                            AttributeName = productAttribute.Name,
                            ColorSquaresRgb = value.ColorSquaresRgb,
                            AttributeValue = value.Name,
                        };
                        m.AttributeIdValueGroup = m.AttributeId + AlgoliaDefaults.Delimiter + m.AttributeName + AlgoliaDefaults.Delimiter + value.Name;
                        model.Add(m);
                    }
                }
                return model;
            });
        }
        protected async Task<IList<ProductCombinationOverviewModel>> PrepareCombinationListModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cache = new CacheKey(AlgoliaModelCacheDefaults.ProductAttrscombinationModelKey, AlgoliaModelCacheDefaults.ProductAttrscombinationPrefixCacheKey);

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, product.Id, (await _workContext.GetWorkingLanguageAsync()).Id);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new List<AlgoliaOverviewModel.ProductCombinationOverviewModel>();
                var allProductAttributeCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
                foreach (var productAttributeCombination in allProductAttributeCombinations)
                {
                    var m = new AlgoliaOverviewModel.ProductCombinationOverviewModel()
                    {
                        CombinationId = productAttributeCombination.Id,
                        GTIN = productAttributeCombination.Gtin,
                        Sku = productAttributeCombination.Sku,
                    };
                    model.Add(m);
                }
                return model;
            });
        }

        protected async Task<IList<AlgoliaOverviewModel.SpecificationModel>> PrepareSpecificationListModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cache = new CacheKey(AlgoliaModelCacheDefaults.ProductSpecsModelKey, AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKey);
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, product.Id, (await _workContext.GetWorkingLanguageAsync()).Id);

            return await (await _cacheManager.GetAsync(cacheKey, async () => (await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id, 0, true))
                .SelectAwait(async psa =>
                {
                    var specAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);
                    var specAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specAttributeOption.SpecificationAttributeId);
                    var m = new AlgoliaOverviewModel.SpecificationModel
                    {
                        OptionId = psa.SpecificationAttributeOptionId,
                        SpecificationAttributeId = specAttributeOption.SpecificationAttributeId,
                        SpecificationAttributeName = specAttribute.Name,
                        ColorSquaresRgb = specAttributeOption.ColorSquaresRgb,
                        AttributeTypeId = psa.AttributeTypeId
                    };

                    switch (psa.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            m.ValueRaw = WebUtility.HtmlEncode(specAttributeOption.Name);
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw = WebUtility.HtmlEncode(psa.CustomValue);
                            break;
                        case SpecificationAttributeType.Hyperlink:
                        case SpecificationAttributeType.CustomHtmlText:
                            m.ValueRaw = psa.CustomValue;
                            break;
                        default:
                            break;
                    }
                    m.SpecificationValueGroup = m.SpecificationAttributeId + AlgoliaDefaults.Delimiter +
                        m.SpecificationAttributeName + AlgoliaDefaults.Delimiter + m.ValueRaw;
                    m.OptionIdSpecificationId = specAttributeOption.Id + AlgoliaDefaults.Delimiter +
                        specAttributeOption.SpecificationAttributeId;
                    return m;
                }))).ToListAsync();
        }

        protected async Task<AlgoliaOverviewModel.VendorModel> PrepareVendorModelAsync(Product product)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return null;

            var model = new AlgoliaOverviewModel.VendorModel
            {
                Name = vendor.Name,
                SeName = await _urlRecordService.GetSeNameAsync(vendor),
                Id = vendor.Id
            };

            return model;
        }

        protected async Task<IList<AlgoliaOverviewModel.CategoryModel>> PrepareCategoryListModelAsync(Product product)
        {
            var model = new List<AlgoliaOverviewModel.CategoryModel>();
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);

            if (productCategories.Any())
            {
                foreach (var productCategory in productCategories)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(productCategory.CategoryId);
                    if (category == null || !category.Published || category.Deleted)
                        continue;

                    var sename = await _urlRecordService.GetSeNameAsync(category);

                    model.Add(new AlgoliaOverviewModel.CategoryModel()
                    {
                        Id = productCategory.CategoryId,
                        Name = category.Name,
                        SeName = sename
                    });
                    if (_algoliaSearchSettings.EnableMultilingualSearch)
                    {
                        var languages = await _languageService.GetAllLanguagesAsync();
                        foreach (var language in languages)
                        {
                            var categoryLocalizedName = await _localizationService.GetLocalizedAsync(category, entity => entity.Name, language.Id, false, false);
                            var categoryLocalizedSeName = await _urlRecordService.GetSeNameAsync(category, language.Id, false, false);

                            if (!string.IsNullOrEmpty(categoryLocalizedName))
                            {
                                model.Add(new AlgoliaOverviewModel.CategoryModel()
                                {
                                    Id = productCategory.CategoryId,
                                    Name = categoryLocalizedName,
                                    SeName = string.IsNullOrEmpty(categoryLocalizedSeName) ? sename : categoryLocalizedSeName
                                });
                            }
                        }
                    }
                }
            }
            return model;
        }

        protected async Task<IList<AlgoliaOverviewModel.ManufacturerModel>> PrepareManufacturerListModelAsync(Product product)
        {
            var model = new List<AlgoliaOverviewModel.ManufacturerModel>();
            var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);

            if (productManufacturers.Any())
            {
                foreach (var productManufacturer in productManufacturers)
                {
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(productManufacturer.ManufacturerId);
                    if (manufacturer == null || !manufacturer.Published || manufacturer.Deleted)
                        continue;

                    var sename = await _urlRecordService.GetSeNameAsync(manufacturer);
                    model.Add(new AlgoliaOverviewModel.ManufacturerModel()
                    {
                        Id = productManufacturer.ManufacturerId,
                        Name = manufacturer.Name,
                        SeName = sename
                    });
                    if (_algoliaSearchSettings.EnableMultilingualSearch)
                    {
                        var languages = await _languageService.GetAllLanguagesAsync();
                        foreach (var language in languages)
                        {
                            var manufacturerLocalizedName = await _localizationService.GetLocalizedAsync(manufacturer, entity => entity.Name, language.Id, false, false);
                            var manufacturerLocalizedSeName = await _urlRecordService.GetSeNameAsync(manufacturer, language.Id, false, false);

                            if (!string.IsNullOrEmpty(manufacturerLocalizedName))
                            {
                                model.Add(new AlgoliaOverviewModel.ManufacturerModel
                                {
                                    Id = productManufacturer.ManufacturerId,
                                    Name = manufacturerLocalizedName,
                                    SeName = string.IsNullOrEmpty(manufacturerLocalizedSeName) ? sename : manufacturerLocalizedSeName
                                });
                            }
                        }
                    }
                }
            }
            return model;
        }

        protected async Task<string> GetAutoCompleteImageUrlAsync(Product product)
        {
            var pictureSize = 50;

            var cache = new CacheKey(AlgoliaModelCacheDefaults.AutoCompletePictureModelKey, AlgoliaModelCacheDefaults.AutoCompletePicturePrefixCacheKey);
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, product.Id, (await _workContext.GetWorkingLanguageAsync()).Id);

            var autoCompleteImageUrl = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                return (await _pictureService.GetPictureUrlAsync(picture, pictureSize)).Url;
            });
            return autoCompleteImageUrl;
        }

        protected Algolia.Search.Index GetIndex(int? orderby = null)
        {
            var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);
            if (!orderby.HasValue || orderby == 0)
                return client.InitIndex(AlgoliaDefaults.DefaultIndexName);
            return client.InitIndex(((AlgoliaSortingEnum)orderby).ToString());
        }

        protected async Task<string> GetFilterStringAsync(IList<int> cids, IList<int> mids, IList<int> vids, IList<FilteredGroupModel> specids,
            IList<FilteredGroupModel> attrids, IList<int> ratings, decimal? minPrice, decimal? maxPrice)
        {
            var sb = new StringBuilder();
            if (_algoliaSearchSettings.AllowManufacturerFilter && cids != null && cids.Any())
                sb.AppendFormat($"(FilterableCategories.Id={string.Join(" OR FilterableCategories.Id=", cids)}) AND ");

            if (_algoliaSearchSettings.AllowManufacturerFilter && mids != null && mids.Any())
                sb.AppendFormat($"(FilterableManufacturers.Id={string.Join(" OR FilterableManufacturers.Id=", mids)}) AND ");

            if (_algoliaSearchSettings.AllowVendorFilter && vids != null && vids.Any())
                sb.AppendFormat($"(FilterableVendor.Id={string.Join(" OR FilterableVendor.Id=", vids)}) AND ");

            if (_algoliaSearchSettings.AllowSpecificationFilter && specids != null && specids.Any())
            {
                var groups = specids.GroupBy(x => x.Id);
                foreach (var group in groups)
                {
                    var idList = group.Select(x => x.OptionId).ToList();
                    sb.AppendFormat($"(FilterableSpecifications.OptionId={string.Join(" OR FilterableSpecifications.OptionId=", idList)}) AND ");
                }
            }

            if (_algoliaSearchSettings.AllowRatingFilter && ratings != null && ratings.Any())
                sb.AppendFormat($"(Rating={string.Join(" OR Rating=", ratings)}) AND ");

            if (_algoliaSearchSettings.AllowPriceRangeFilter)
            {
                if (maxPrice.HasValue && maxPrice > 0)
                    sb.AppendFormat($"PriceValue <= {maxPrice.Value} AND ");
                if (minPrice.HasValue && minPrice > 0)
                    sb.AppendFormat($"PriceValue >= {minPrice.Value} AND ");
            }

            if (!_catalogSettings.IgnoreStoreLimitations)
            {
                sb.AppendFormat($"(LimitedToStores = 0 OR Stores = {(await _storeContext.GetCurrentStoreAsync()).Id}) AND ");
            }


            var filterString = sb.ToString().Trim();
            if (filterString.EndsWith("AND"))
                filterString = filterString.Substring(0, sb.Length - 4).Trim();

            return filterString;
        }

        #endregion

        #region Methods

        public async Task UploadProductsAsync(UploadProductModel model)
        {
            var index = GetIndex();

            var pageIndex = 0;
            var currentPageProducts = 0;
            var totalProducts = 0;
            var totalPages = 0;
            var uploaded = 0;
            var failed = 0;

            try
            {
                while (true)
                {
                    await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, 0, failed, uploaded, -10, "Products fetching from database...");
                    var products = await _algoliaCatalogService.SearchProductsAsync(fromId: model.FromId, toId: model.ToId, pageIndex: pageIndex, pageSize: 100);
                    if (products == null || products.Count == 0)
                        break;

                    currentPageProducts = products.Count;
                    totalProducts = products.TotalCount;
                    totalPages = products.TotalPages;

                    var binding = 0;
                    var objects = new List<JObject>();

                    foreach (var product in products)
                    {
                        try
                        {
                            await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding + 1, failed, uploaded, 110);
                            objects.Add(await GetProductModelObjectAsync(product));

                            binding++;
                            await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, 10);
                        }
                        catch (Exception ex)
                        {
                            await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message + ", Product Id = " + product.Id, ex);
                            failed++;

                            await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, -1, ex.Message);
                            continue;
                        }
                    }

                    await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, 20);
                    var res = index.PartialUpdateObjects(objects, true);

                    uploaded += binding;
                    await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, 10);
                    pageIndex++;
                }
                await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, 0, failed, uploaded, 100);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message, ex);
                await _productUploadHub.UploadProductsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, 0, failed, uploaded, -1, ex.Message);
            }
        }

        public async Task UpdateIndicesAsync(ConfigurationModel model)
        {
            try
            {
                var client = new AlgoliaClient(_algoliaSearchSettings.ApplicationId, _algoliaSearchSettings.AdminKey);
                var defaultIndex = GetDefaultIndex(client, model, out var settings);

                var replicas = _algoliaSearchSettings.AllowProductSorting ? _algoliaSearchSettings.AllowedSortingOptions : new List<int>();
                foreach (var index in replicas)
                {
                    var enumItem = (AlgoliaSortingEnum)index;
                    var replicaIndex = client.InitIndex(enumItem.ToString());
                    var sortType = enumItem switch
                    {
                        AlgoliaSortingEnum.NameAsc => "asc(Name)",
                        AlgoliaSortingEnum.NameDesc => "desc(Name)",
                        AlgoliaSortingEnum.PriceAsc => "asc(PriceValue)",
                        AlgoliaSortingEnum.PriceDesc => "desc(PriceValue)",
                        AlgoliaSortingEnum.CreatedOn => "desc(CreatedOn)",
                        _ => "",
                    };
                    dynamic replicaSettings = new JObject();
                    replicaSettings.customRanking = new JArray(new string[] { sortType });
                    replicaSettings.ranking = new JArray(new string[] { "typo", "geo", "words", "filters", "proximity", "attribute", "exact", "custom" });
                    replicaSettings.searchableAttributes = settings.searchableAttributes;
                    replicaSettings.attributesForFaceting = settings.attributesForFaceting;

                    replicaIndex.SetSettings(replicaSettings);
                }

                settings.replicas = new JArray(replicas.Select(x => ((AlgoliaSortingEnum)x).ToString()));
                var setSettingsResponse = defaultIndex.SetSettings(settings);
                if (setSettingsResponse.taskID != null)
                    defaultIndex.WaitTask(setSettingsResponse.taskID.ToString());

                var removeIndexItems = Enum.GetValues(typeof(AlgoliaSortingEnum)).Cast<int>().Where(x => !replicas.Contains(x) && x != 0);
                foreach (var index in removeIndexItems)
                    client.DeleteIndex(((AlgoliaSortingEnum)index).ToString());
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message, ex);
            }
        }

        public void ClearIndex()
        {
            GetIndex().ClearIndex();
        }

        public async Task UpdateAlgoliaItemAsync()
        {
            var index = GetIndex();
            var pageIndex = 0;

            var productItems = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync("Product");
            var categoryItems = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync("Category");
            var manufacturerItems = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync("Manufacturer");
            var vendorItems = await _algoliaUpdatableItemService.SearchAlgoliaUpdatableItemsAsync("Vendor");

            var pIds = productItems.Any() ? productItems.Select(x => x.EntityId).ToList() : new List<int>();
            var cIds = categoryItems.Any() ? categoryItems.Select(x => x.EntityId).ToList() : new List<int>();
            var mIds = manufacturerItems.Any() ? manufacturerItems.Select(x => x.EntityId).ToList() : new List<int>();
            var vIds = vendorItems.Any() ? vendorItems.Select(x => x.EntityId).ToList() : new List<int>();

            while (true)
            {
                try
                {
                    var products = await _algoliaCatalogService.GetProductsByEntityIdsAsync(
                        productIds: pIds,
                        categoryIds: cIds,
                        manufacturerIds: mIds,
                        vendorIds: vIds,
                        pageIndex: pageIndex,
                        pageSize: 100);

                    var objects = new List<JObject>();

                    foreach (var product in products)
                    {
                        try
                        {
                            objects.Add(await GetProductModelObjectAsync(product));
                        }
                        catch (Exception ex)
                        {
                            await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message + ", Product Id = " + product.Id, ex);
                            continue;
                        }
                    }
                    if (products != null || products.Count != 0)
                        index.PartialUpdateObjects(objects, true);
                    await _algoliaUpdatableItemService.DeleteAlgoliaUpdatableItemsByProductsAsync(products);
                    if (!objects.Any())
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message, ex);
                }
                pageIndex++;
            }
            pageIndex = 0;
            while (true)
            {
                try
                {
                    var products = await _algoliaCatalogService.GetProductsByEntityIdsAsync(
                        productIds: pIds,
                        categoryIds: cIds,
                        manufacturerIds: mIds,
                        vendorIds: vIds,
                        pageIndex: pageIndex,
                        pageSize: 100,
                        deletedOrUnpublishProduct: true);

                    var pids = await products.Select(p => p.Id.ToString()).ToListAsync();
                    if (pids == null || pids.Count == 0)
                        break;

                    await index.DeleteObjectsAsync(pids);
                    await _algoliaUpdatableItemService.DeleteAlgoliaUpdatableItemsByProductsAsync(products);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("AlgoliaSearch: " + ex.Message, ex);
                }
                pageIndex++;
            }
        }

        public async Task<IPagedList<ProductOverviewModel>> SearchProductsAsync(string searchTerms = "", IList<int> cids = null, IList<int> mids = null,
            IList<int> vids = null, IList<FilteredGroupModel> specids = null, IList<FilteredGroupModel> attrids = null,
            IList<int> ratings = null, decimal? minPrice = null, decimal? maxPrice = null, int? orderby = null, int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var index = GetIndex(orderby);
            var searchObject = index.Search(new Query(searchTerms)
                .SetFilters(await GetFilterStringAsync(cids, mids, vids, specids, attrids, ratings, minPrice, maxPrice))
                .SetPage(pageIndex)
                .SetNbHitsPerPage(pageSize));

            var ids = searchObject["hits"].Select(x => int.Parse(x["Id"].ToString())).ToList();
            var products = await _algoliaCatalogService.SearchProductsAsync(productIds: ids, inProductIdsOnly: true);

            var productOverviewModels = await _productModelFactory.PrepareProductOverviewModelsAsync(products);

            var totalItems = int.Parse(searchObject["nbHits"].ToString());
            var totalPages = int.Parse(searchObject["nbPages"].ToString());

            return new PagedList<ProductOverviewModel>(productOverviewModels.ToList(), pageIndex, pageSize, totalItems);
        }

        public async Task<AlgoliaFilters> GetAlgoliaFiltersAsync(string searchTerms)
        {
            var model = new AlgoliaFilters();

            var filters = "";
            if (!_catalogSettings.IgnoreStoreLimitations)
            {
                filters = $"LimitedToStores = 0 OR Stores = {(await _storeContext.GetCurrentStoreAsync()).Id}";
            }

            var index = GetIndex();
            var res = index.Search(new Query(searchTerms)
                     .SetFilters(filters)
                     .SetFacets(AlgoliaDefaults.FacetedAttributes)
                     .EnableFacetingAfterDistinct(true)
                     .SetFacetFilters(AlgoliaDefaults.FacetedAttributes));

            if (_algoliaSearchSettings.AllowPriceRangeFilter && res["facets"]["Price"] != null)
            {
                var json = res["facets"]["Price"].ToString();
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);

                var max = decimal.MinValue;
                var min = decimal.MaxValue;

                foreach (var item in values)
                {
                    try
                    {
                        var val = decimal.Parse(item.Key, NumberStyles.Float, new CultureInfo("en-US"));
                        if (max < val)
                            max = val;

                        if (min > val)
                            min = val;
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync(ex.Message + ", Value=" + item.Key, ex);
                        continue;
                    }
                }
                if (min != decimal.MaxValue && max != decimal.MinValue)
                {
                    model.MaxPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(max, await _workContext.GetWorkingCurrencyAsync());
                    model.MinPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(min, await _workContext.GetWorkingCurrencyAsync());
                }
            }

            if (_algoliaSearchSettings.AllowVendorFilter && res["facets"]["FilterableVendor.Id"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["FilterableVendor.Id"].ToString()).ToList();
                model.AvailableVendors = values
                    .Take(_algoliaSearchSettings.MaximumVendorsShowInFilter)
                    .Select(x => new FilterItemModel()
                    {
                        Count = x.Value,
                        Id = int.Parse(x.Key)
                    }).ToList();
            }

            if (_algoliaSearchSettings.AllowManufacturerFilter && res["facets"]["FilterableManufacturers.Id"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["FilterableManufacturers.Id"].ToString()).ToList();
                model.AvailableManufacturers = values.Take(_algoliaSearchSettings.MaximumManufacturersShowInFilter)
                    .Select(x => new FilterItemModel()
                    {
                        Count = x.Value,
                        Id = int.Parse(x.Key)
                    }).ToList();
            }

            if (_algoliaSearchSettings.AllowCategoryFilter && res["facets"]["FilterableCategories.Id"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["FilterableCategories.Id"].ToString()).ToList();
                model.AvailableCategories = values.Take(_algoliaSearchSettings.MaximumCategoriesShowInFilter)
                    .Select(x => new FilterItemModel()
                    {
                        Count = x.Value,
                        Id = int.Parse(x.Key)
                    }).ToList();
            }

            if (_algoliaSearchSettings.AllowSpecificationFilter && res["facets"]["FilterableSpecifications.OptionIdSpecificationId"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["FilterableSpecifications.OptionIdSpecificationId"].ToString()).ToList();
                model.AvailableSpecifications = values.Select(x => new
                {
                    Count = x.Value,
                    OS = x.Key.Split(AlgoliaDefaults.Delimiter, StringSplitOptions.RemoveEmptyEntries)
                })
                .GroupBy(x => x.OS[1])
                .SelectMany(g => g.Take(_algoliaSearchSettings.MaximumSpecificationsShowInFilter))
                .Select(x => new FilterItemModel()
                {
                    Count = x.Count,
                    Id = int.Parse(x.OS[0])
                })
                .ToList();
            }

            if (_algoliaSearchSettings.AllowRatingFilter && res["facets"]["Rating"] != null)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, int>>(res["facets"]["Rating"].ToString());
                for (var i = 1; i <= 5; i++)
                    model.AvailableRatings.Add(new FilterItemModel() { Id = i, Count = values.TryGetValue(i.ToString(), out var v1) ? v1 : 0 });
            }

            return model;
        }

        #endregion
    }
}
