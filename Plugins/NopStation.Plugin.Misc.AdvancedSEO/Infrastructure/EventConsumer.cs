using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IISIntegration;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Helpers;
using NopStation.Plugin.Misc.AdvancedSEO.Services;

namespace NopStation.Plugin.Misc.AdvancedSEO.Infrastructure
{
    public class EventConsumer : IConsumer<ModelPreparedEvent<BaseNopModel>>, 
        //IConsumer<EntityInsertedEvent<BaseEntity>>, 
        //IConsumer<EntityUpdatedEvent <BaseEntity>>, 
        //IConsumer<EntityDeletedEvent <BaseEntity>>, 

        IConsumer<EntityInsertedEvent<Category>>, 
        IConsumer<EntityUpdatedEvent <Category>>, 
        IConsumer<EntityDeletedEvent <Category>>, 

        IConsumer<EntityInsertedEvent<CategorySEOTemplate>>, 
        IConsumer<EntityUpdatedEvent <CategorySEOTemplate>>, 
        IConsumer<EntityDeletedEvent <CategorySEOTemplate>>, 

        IConsumer<EntityInsertedEvent<CategoryCategorySEOTemplateMapping>>, 
        IConsumer<EntityUpdatedEvent <CategoryCategorySEOTemplateMapping>>, 
        IConsumer<EntityDeletedEvent <CategoryCategorySEOTemplateMapping>>, 

        IConsumer<EntityInsertedEvent<Manufacturer>>, 
        IConsumer<EntityUpdatedEvent <Manufacturer>>, 
        IConsumer<EntityDeletedEvent <Manufacturer>>, 

        IConsumer<EntityInsertedEvent<ManufacturerSEOTemplate>>, 
        IConsumer<EntityUpdatedEvent <ManufacturerSEOTemplate>>, 
        IConsumer<EntityDeletedEvent <ManufacturerSEOTemplate>>, 

        IConsumer<EntityInsertedEvent<ManufacturerManufacturerSEOTemplateMapping>>, 
        IConsumer<EntityUpdatedEvent <ManufacturerManufacturerSEOTemplateMapping>>, 
        IConsumer<EntityDeletedEvent <ManufacturerManufacturerSEOTemplateMapping>>, 

        IConsumer<EntityInsertedEvent<Product>>, 
        IConsumer<EntityUpdatedEvent <Product>>, 
        IConsumer<EntityDeletedEvent <Product>>, 

        IConsumer<EntityInsertedEvent<ProductSEOTemplate>>, 
        IConsumer<EntityUpdatedEvent <ProductSEOTemplate>>, 
        IConsumer<EntityDeletedEvent <ProductSEOTemplate>>, 

        IConsumer<EntityInsertedEvent<ProductProductSEOTemplateMapping>>, 
        IConsumer<EntityUpdatedEvent <ProductProductSEOTemplateMapping>>, 
        IConsumer<EntityDeletedEvent <ProductProductSEOTemplateMapping>>,

        IConsumer<EntityInsertedEvent<ProductCategory>>, 
        IConsumer<EntityUpdatedEvent <ProductCategory>>, 
        IConsumer<EntityDeletedEvent <ProductCategory>>,

        IConsumer<EntityInsertedEvent<ProductManufacturer>>, 
        IConsumer<EntityUpdatedEvent <ProductManufacturer>>, 
        IConsumer<EntityDeletedEvent <ProductManufacturer>>,

        IConsumer<EntityInsertedEvent<ProductTag>>, 
        IConsumer<EntityUpdatedEvent <ProductTag>>, 
        IConsumer<EntityDeletedEvent <ProductTag>>,

        IConsumer<EntityInsertedEvent<ProductProductTagMapping>>, 
        IConsumer<EntityUpdatedEvent <ProductProductTagMapping>>, 
        IConsumer<EntityDeletedEvent <ProductProductTagMapping>>
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryCategorySEOTemplateMappingService _categoryCategorySEOTemplateMappingService;
        private readonly ICategorySEOTemplateService _categorySEOTemplateService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IManufacturerService _manufacturerService;
        private readonly IManufacturerManufacturerSEOTemplateMappingService _manufacturerManufacturerSEOTemplateMappingService;
        private readonly IManufacturerSEOTemplateService _manufacturerSEOTemplateService;
        private readonly IProductService _productService;
        private readonly IProductSEOTemplateService _productSEOTemplateService;
        private readonly IProductProductSEOTemplateMappingService _productProductSEOTemplateMappingService;
        private readonly ISEOTokenProvider _sEOTokenProvider;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITokenizer _tokenizer;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        #region Fields



        #endregion

        #region Ctor

        public EventConsumer(
            ICategoryService categoryService,
            ICategoryCategorySEOTemplateMappingService categoryCategorySEOTemplateMappingService,
            ICategorySEOTemplateService categorySEOTemplateService,
            ILocalizationService localizationService,
            ILogger logger,
            IManufacturerService manufacturerService,
            IManufacturerManufacturerSEOTemplateMappingService manufacturerManufacturerSEOTemplateMappingService,
            IManufacturerSEOTemplateService manufacturerSEOTemplateService,
            IProductService productService,
            IProductSEOTemplateService productSEOTemplateService,
            IProductProductSEOTemplateMappingService productProductSEOTemplateMappingService,
            ISEOTokenProvider sEOTokenProvider,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            ITokenizer tokenizer,
            IWorkContext workContext,
            CatalogSettings catalogSettings
            )
        {
            _categoryService = categoryService;
            _categoryCategorySEOTemplateMappingService = categoryCategorySEOTemplateMappingService;
            _categorySEOTemplateService = categorySEOTemplateService;
            _localizationService = localizationService;
            _logger = logger;
            _manufacturerService = manufacturerService;
            _manufacturerManufacturerSEOTemplateMappingService = manufacturerManufacturerSEOTemplateMappingService;
            _manufacturerSEOTemplateService = manufacturerSEOTemplateService;
            _productService = productService;
            _productSEOTemplateService = productSEOTemplateService;
            _productProductSEOTemplateMappingService = productProductSEOTemplateMappingService;
            _sEOTokenProvider = sEOTokenProvider;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _tokenizer = tokenizer;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Utilites

        /// <summary>
        /// Handle function and get result
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="function">Function</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        private async Task<TResult> HandleFunctionAsync<TResult>(Func<Task<TResult>> function)
        {
            try
            {
                //check whether the plugin is active
                //if (!await PluginActiveAsync())
                //    return default;

                //invoke function
                return await function();
            }
            catch (Exception exception)
            {
                //get a short error message
                var detailedException = exception;
                do
                {
                    detailedException = detailedException.InnerException;
                } while (detailedException?.InnerException != null);


                //log errors
                var error = $"{AdvancedSEOPluginDefaults.PluginSystemName} error: {Environment.NewLine}{exception.Message}";
                await _logger.ErrorAsync(error, exception, await _workContext.GetCurrentCustomerAsync());

                return default;
            }
        }

        /// <summary>
        /// Prepare script to track "ViewContent" event
        /// </summary>
        /// <param name="model">Product details model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task HandleCategoryModelEventAsync(CategoryModel model)
        {
            await HandleFunctionAsync(async () =>
            {

                var language = await _workContext.GetCurrentCustomerAsync();
                var category = await _categoryService.GetCategoryByIdAsync(model.Id);
                //var categoryTitleTokens = new List<Token>();
                var categoryMateTitleTokens = new List<Token>();
                //var categoryDescriptionTokens = new List<Token>();
                var categoryMetaDescriptionTokens = new List<Token>();
                var categoryKeywordsTokens = new List<Token>();
                //await _sEOTokenProvider.AddCategoryMetaTitleTokensAsync(categoryTitleTokens, category, 0);
                await _sEOTokenProvider.AddCategoryMetaTitleTokensAsync(categoryMateTitleTokens, category, 0);
                //await _sEOTokenProvider.AddCategoryDescriptionTokensAsync(categoryDescriptionTokens, category, 0);
                await _sEOTokenProvider.AddCategoryMetaDescriptionTokensAsync(categoryMetaDescriptionTokens, category, 0);
                await _sEOTokenProvider.AddCategoryKeywordsTokensAsync(categoryKeywordsTokens, category, 0);
                
                var store = await _storeContext.GetCurrentStoreAsync();

                var cacheKey = _cacheManager.PrepareKeyForDefaultCache(AdvancedSEOPluginDefaults.CategorySEOTemplateCacheKey,
                    category,
                    language,
                    store
                    );

                var seoData = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    var categorySEOTemplate = (await _categoryCategorySEOTemplateMappingService.GetAllMappedCategorySEOTemplateByCategoryIdAsync(category.Id, store.Id)).FirstOrDefault();
                    if (categorySEOTemplate == null)
                        categorySEOTemplate = (await _categorySEOTemplateService.GetAllCategorySEOTemplateAsync(storeId: store.Id, isGlobalTemplate: true, isActive: true)).FirstOrDefault();
                    if (categorySEOTemplate == null)
                        return null;

                    var seoData = new SEODataModel()
                    {
                        MetaTitle = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(categorySEOTemplate, x => x.SEOTitleTemplate, language.Id), categoryMateTitleTokens, false),
                        MetaDescription = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(categorySEOTemplate, x => x.SEODescriptionTemplate, language.Id), categoryMetaDescriptionTokens, false),
                        MetaKeywords = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(categorySEOTemplate, x => x.SEOKeywordsTemplate, language.Id), categoryKeywordsTokens, false),
                    };

                    if (categorySEOTemplate.IncludeProductNamesOnKeyword)
                    {

                        var categoryIds = new List<int> { category.Id };

                        //include subcategories
                        if (_catalogSettings.ShowProductsFromSubcategories)
                            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, store.Id));

                        var productNames = (await _productService.SearchProductsAsync(
                                       0,
                                       categorySEOTemplate.MaxNumberOfProductToInclude,
                                       categoryIds: categoryIds,
                                       storeId: store.Id,
                                       visibleIndividuallyOnly: true,
                                       excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                                       orderBy: ProductSortingEnum.Position)).Select(p => p.Name);
                        
                        if (productNames.Any())
                        {
                            var productStr = string.Join(", ", productNames);
                            seoData.MetaKeywords = string.IsNullOrEmpty(seoData.MetaKeywords)? productStr : seoData.MetaKeywords + ", " + productStr;
                        }
                    }
                    return seoData;
                });

                if (seoData != null)
                {
                    model.MetaTitle = seoData?.MetaTitle;
                    model.MetaDescription = seoData?.MetaDescription;
                    model.MetaKeywords = seoData?.MetaKeywords;
                }
                return true;
            });
        }

        /// <summary>
        /// Prepare script to track "ViewContent" event
        /// </summary>
        /// <param name="model">Product details model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task HandleProductModelEventAsync(ProductDetailsModel model)
        {
            await HandleFunctionAsync(async () =>
            {
                var language = await _workContext.GetCurrentCustomerAsync();
                var product = await _productService.GetProductByIdAsync(model.Id);
                //var productTitleTokens = new List<Token>();
                var productMateTitleTokens = new List<Token>();
                //var productDescriptionTokens = new List<Token>();
                var productMetaDescriptionTokens = new List<Token>();
                var productKeywordsTokens = new List<Token>();
                //await _sEOTokenProvider.AddProductMetaTitleTokensAsync(productTitleTokens, product, 0);
                await _sEOTokenProvider.AddProductMetaTitleTokensAsync(productMateTitleTokens, product, 0);
                //await _sEOTokenProvider.AddProductDescriptionTokensAsync(productDescriptionTokens, product, 0);
                await _sEOTokenProvider.AddProductMetaDescriptionTokensAsync(productMetaDescriptionTokens, product, 0);
                await _sEOTokenProvider.AddProductKeywordsTokensAsync(productKeywordsTokens, product, 0);

                var store = await _storeContext.GetCurrentStoreAsync();

                var cacheKey = _cacheManager.PrepareKeyForDefaultCache(AdvancedSEOPluginDefaults.ProductSEOTemplateCacheKey,
                    product,
                    language,
                    store
                    );

                var seoData = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    var productSEOTemplate = (await _productProductSEOTemplateMappingService.GetAllMappedProductSEOTemplateByProductIdAsync(product.Id, store.Id)).FirstOrDefault();
                    if (productSEOTemplate == null)
                        productSEOTemplate = (await _productSEOTemplateService.GetAllProductSEOTemplateAsync(storeId: store.Id, isGlobalTemplate: true, isActive: true)).FirstOrDefault();
                    if (productSEOTemplate == null)
                        return null;

                    //model.MetaTitle = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(productSEOTemplate, x => x.SEOTitleTemplate, language.Id), productMateTitleTokens, false);
                    //model.MetaDescription = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(productSEOTemplate, x => x.SEODescriptionTemplate, language.Id), productMetaDescriptionTokens, false);
                    //model.MetaKeywords = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(productSEOTemplate, x => x.SEOKeywordsTemplate, language.Id), productKeywordsTokens, false);
                    //return model;

                    var seoData = new SEODataModel()
                    {
                        MetaTitle = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(productSEOTemplate, x => x.SEOTitleTemplate, language.Id), productMateTitleTokens, false) ?? "",
                        MetaDescription = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(productSEOTemplate, x => x.SEODescriptionTemplate, language.Id), productMetaDescriptionTokens, false) ?? "",
                        MetaKeywords = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(productSEOTemplate, x => x.SEOKeywordsTemplate, language.Id), productKeywordsTokens, false) ?? "",
                    };

                    if(productSEOTemplate.IncludeProductTagsOnKeyword && model.ProductTags.Any())
                    {
                        var productTagsStr = string.Join(", ", model.ProductTags.Select(pt => pt.Name));
                        seoData.MetaKeywords = string.IsNullOrEmpty(seoData.MetaKeywords) ? productTagsStr : seoData.MetaKeywords + ", " + productTagsStr;
                    }

                    if(productSEOTemplate.IncludeCategoryNamesOnKeyword && model.Breadcrumb.CategoryBreadcrumb.Any())
                    {
                        var productCategoryStr = string.Join(", ", model.Breadcrumb.CategoryBreadcrumb.Select(pt => pt.Name));
                        seoData.MetaKeywords = string.IsNullOrEmpty(seoData.MetaKeywords) ? productCategoryStr : seoData.MetaKeywords + ", " + productCategoryStr;
                    }

                    if(productSEOTemplate.IncludeManufacturerNamesOnKeyword && model.ProductManufacturers.Any())
                    {
                        var productManufacturerStr = string.Join(", ", model.ProductManufacturers.Select(pt => pt.Name));
                        seoData.MetaKeywords = string.IsNullOrEmpty(seoData.MetaKeywords) ? productManufacturerStr : seoData.MetaKeywords + ", " + productManufacturerStr;
                    }
                    
                    if (productSEOTemplate.IncludeVendorNamesOnKeyword && !string.IsNullOrEmpty(model.VendorModel.Name))
                        seoData.MetaKeywords = string.IsNullOrEmpty(seoData.MetaKeywords) ? model.VendorModel.Name : seoData.MetaKeywords + ", " + model.VendorModel.Name;

                    return seoData;
                });

                if (seoData != null)
                {
                    model.MetaTitle = seoData.MetaTitle;
                    model.MetaDescription = seoData.MetaDescription;
                    model.MetaKeywords = seoData.MetaKeywords;
                }

                return true;
            });
        }

        /// <summary>
        /// Prepare script to track "ViewContent" event
        /// </summary>
        /// <param name="model">Product details model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task HandleManufacturerModelEventAsync(ManufacturerModel model)
        {
            await HandleFunctionAsync(async () =>
            {
                var language = await _workContext.GetCurrentCustomerAsync();
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(model.Id);
                //var manufacturerTitleTokens = new List<Token>();
                var manufacturerMateTitleTokens = new List<Token>();
                //var manufacturerDescriptionTokens = new List<Token>();
                var manufacturerMetaDescriptionTokens = new List<Token>();
                var manufacturerKeywordsTokens = new List<Token>();
                //await _sEOTokenProvider.AddManufacturerMetaTitleTokensAsync(manufacturerTitleTokens, manufacturer, 0);
                await _sEOTokenProvider.AddManufacturerMetaTitleTokensAsync(manufacturerMateTitleTokens, manufacturer, 0);
                //await _sEOTokenProvider.AddManufacturerDescriptionTokensAsync(manufacturerDescriptionTokens, manufacturer, 0);
                await _sEOTokenProvider.AddManufacturerMetaDescriptionTokensAsync(manufacturerMetaDescriptionTokens, manufacturer, 0);
                await _sEOTokenProvider.AddManufacturerKeywordsTokensAsync(manufacturerKeywordsTokens, manufacturer, 0);

                var store = await _storeContext.GetCurrentStoreAsync();
                var cacheKey = _cacheManager.PrepareKeyForDefaultCache(AdvancedSEOPluginDefaults.ManufacturerSEOTemplateCacheKey,
                    manufacturer,
                    language,
                    store
                    );

                var seoData = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    var manufacturerSEOTemplate = (await _manufacturerManufacturerSEOTemplateMappingService.GetAllMappedManufacturerSEOTemplateByManufacturerIdAsync(manufacturer.Id, store.Id)).FirstOrDefault();
                    if (manufacturerSEOTemplate == null)
                        manufacturerSEOTemplate = (await _manufacturerSEOTemplateService.GetAllManufacturerSEOTemplateAsync(storeId: store.Id, isGlobalTemplate: true, isActive: true)).FirstOrDefault();
                    if (manufacturerSEOTemplate == null)
                        return null;

                    var seoData = new SEODataModel()
                    {

                        MetaTitle = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(manufacturerSEOTemplate, x => x.SEOTitleTemplate, language.Id), manufacturerMateTitleTokens, false),
                        MetaDescription = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(manufacturerSEOTemplate, x => x.SEODescriptionTemplate, language.Id), manufacturerMetaDescriptionTokens, false),
                        MetaKeywords = _tokenizer.Replace(await _localizationService.GetLocalizedAsync(manufacturerSEOTemplate, x => x.SEOKeywordsTemplate, language.Id), manufacturerKeywordsTokens, false),
                    };

                    if (manufacturerSEOTemplate.IncludeProductNamesOnKeyword)
                    {
                        var productNames = (await _productService.SearchProductsAsync(
                                       0,
                                       manufacturerSEOTemplate.MaxNumberOfProductToInclude,
                                       manufacturerIds: new List<int> { manufacturer.Id },
                                       storeId: store.Id,
                                       visibleIndividuallyOnly: true,
                                       excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                                       orderBy: ProductSortingEnum.Position)).Select(p => p.Name);
                        
                        if (productNames.Any())
                        {
                            var productStr = string.Join(", ", productNames);
                            seoData.MetaKeywords = string.IsNullOrEmpty(seoData.MetaKeywords) ? productStr : seoData.MetaKeywords + ", " + productStr;
                        }
                    }
                    return seoData;
                });

                if (seoData != null)
                {
                    model.MetaTitle= seoData.MetaTitle;
                    model.MetaDescription= seoData.MetaDescription;
                    model.MetaKeywords= seoData.MetaKeywords;
                }
                
                return true;
            });
        }


        #endregion

        #region Methods

        /// <summary>
        /// Handle product details model prepared event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
        {
            if (eventMessage?.Model is CategoryModel categoryModel)
                await HandleCategoryModelEventAsync(categoryModel);

            else if (eventMessage?.Model is ProductDetailsModel productDetailsModel)
                await HandleProductModelEventAsync(productDetailsModel);

            if (eventMessage?.Model is ManufacturerModel manufacturerModel)
                await HandleManufacturerModelEventAsync(manufacturerModel);

            //await _cacheManager.RemoveByPrefixAsync(SmsDefaults.MessageTemplatesPrefixCacheKey);
        }

        //public async Task HandleEventAsync(EntityUpdatedEvent<BaseEntity> eventMessage)
        //{
        //    if (eventMessage?.Entity is Category category)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is CategorySEOTemplate categorySEOTemplate)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
            
        //    else if (eventMessage?.Entity is Manufacturer manufacturer)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ManufacturerSEOTemplate manufacturerSEOTemplate)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is Product product)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ProductSEOTemplate productSEOTemplate)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ProductProductSEOTemplateMapping productProductSEOTemplateMapping)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        //}

        //public async Task HandleEventAsync(EntityDeletedEvent<BaseEntity> eventMessage)
        //{
        //    if (eventMessage?.Entity is Category category)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is CategorySEOTemplate categorySEOTemplate)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is Manufacturer manufacturer)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ManufacturerSEOTemplate manufacturerSEOTemplate)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is Product product)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ProductSEOTemplate productSEOTemplate)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ProductProductSEOTemplateMapping productProductSEOTemplateMapping)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        //}

        //public async Task HandleEventAsync(EntityInsertedEvent<BaseEntity> eventMessage)
        //{
        //    if (eventMessage?.Entity is Category category)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is CategorySEOTemplate categorySEOTemplate)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is CategoryCategorySEOTemplateMapping categoryCategorySEOTemplateMapping)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is Manufacturer manufacturer)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ManufacturerSEOTemplate manufacturerSEOTemplate)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is Product product)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ProductSEOTemplate productSEOTemplate)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);

        //    else if (eventMessage?.Entity is ProductProductSEOTemplateMapping productProductSEOTemplateMapping)
        //        await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        //}

        public async Task HandleEventAsync(EntityDeletedEvent<ProductSEOTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductProductSEOTemplateMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductProductSEOTemplateMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductProductSEOTemplateMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductSEOTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductSEOTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<CategorySEOTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<CategorySEOTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<CategorySEOTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<CategoryCategorySEOTemplateMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<CategoryCategorySEOTemplateMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<CategoryCategorySEOTemplateMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Manufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ManufacturerSEOTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ManufacturerSEOTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ManufacturerSEOTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ManufacturerManufacturerSEOTemplateMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ManufacturerManufacturerSEOTemplateMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ManufacturerManufacturerSEOTemplateMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductCategory> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductCategory> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductCategory> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.CategorySeoTemplateCacheKeyPrefix);
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductManufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductManufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductManufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ManufacturerSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductTag> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTag> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductTag> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ProductProductTagMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ProductProductTagMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ProductProductTagMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AdvancedSEOPluginDefaults.ProductSeoTemplateCacheKeyPrefix);
        }

        #endregion


    }
}
