using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Cms;
using Nop.Services.Events;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.WebApi.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ApiCacheEventConsumer :
        //languages
        IConsumer<EntityInsertedEvent<Language>>,
        IConsumer<EntityUpdatedEvent<Language>>,
        IConsumer<EntityDeletedEvent<Language>>,
        //currencies
        IConsumer<EntityInsertedEvent<Currency>>,
        IConsumer<EntityUpdatedEvent<Currency>>,
        IConsumer<EntityDeletedEvent<Currency>>,
        //settings
        IConsumer<EntityUpdatedEvent<Setting>>,
        //manufacturers
        IConsumer<EntityInsertedEvent<Manufacturer>>,
        IConsumer<EntityUpdatedEvent<Manufacturer>>,
        IConsumer<EntityDeletedEvent<Manufacturer>>,
        //vendors
        IConsumer<EntityInsertedEvent<Vendor>>,
        IConsumer<EntityUpdatedEvent<Vendor>>,
        IConsumer<EntityDeletedEvent<Vendor>>,
        //product manufacturers
        IConsumer<EntityInsertedEvent<ProductManufacturer>>,
        IConsumer<EntityUpdatedEvent<ProductManufacturer>>,
        IConsumer<EntityDeletedEvent<ProductManufacturer>>,
        //categories
        IConsumer<EntityInsertedEvent<Category>>,
        IConsumer<EntityUpdatedEvent<Category>>,
        IConsumer<EntityDeletedEvent<Category>>,
        //product categories
        IConsumer<EntityInsertedEvent<ProductCategory>>,
        IConsumer<EntityUpdatedEvent<ProductCategory>>,
        IConsumer<EntityDeletedEvent<ProductCategory>>,
        //products
        IConsumer<EntityInsertedEvent<Product>>,
        IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityDeletedEvent<Product>>,
        //related product
        IConsumer<EntityInsertedEvent<RelatedProduct>>,
        IConsumer<EntityUpdatedEvent<RelatedProduct>>,
        IConsumer<EntityDeletedEvent<RelatedProduct>>,
        //product tags
        IConsumer<EntityInsertedEvent<ProductTag>>,
        IConsumer<EntityUpdatedEvent<ProductTag>>,
        IConsumer<EntityDeletedEvent<ProductTag>>,
        //specification attributes
        IConsumer<EntityUpdatedEvent<SpecificationAttribute>>,
        IConsumer<EntityDeletedEvent<SpecificationAttribute>>,
        //specification attribute options
        IConsumer<EntityUpdatedEvent<SpecificationAttributeOption>>,
        IConsumer<EntityDeletedEvent<SpecificationAttributeOption>>,
        //Product specification attribute
        IConsumer<EntityInsertedEvent<ProductSpecificationAttribute>>,
        IConsumer<EntityUpdatedEvent<ProductSpecificationAttribute>>,
        IConsumer<EntityDeletedEvent<ProductSpecificationAttribute>>,
        //Product attribute values
        IConsumer<EntityUpdatedEvent<ProductAttributeValue>>,
        //Topics
        IConsumer<EntityInsertedEvent<Topic>>,
        IConsumer<EntityUpdatedEvent<Topic>>,
        IConsumer<EntityDeletedEvent<Topic>>,
        //Orders
        IConsumer<EntityInsertedEvent<Order>>,
        IConsumer<EntityUpdatedEvent<Order>>,
        IConsumer<EntityDeletedEvent<Order>>,
        //Picture
        IConsumer<EntityInsertedEvent<Picture>>,
        IConsumer<EntityUpdatedEvent<Picture>>,
        IConsumer<EntityDeletedEvent<Picture>>,
        //Product picture mapping
        IConsumer<EntityInsertedEvent<ProductPicture>>,
        IConsumer<EntityUpdatedEvent<ProductPicture>>,
        IConsumer<EntityDeletedEvent<ProductPicture>>,
        //Product review
        IConsumer<EntityDeletedEvent<ProductReview>>,
        //polls
        IConsumer<EntityInsertedEvent<Poll>>,
        IConsumer<EntityUpdatedEvent<Poll>>,
        IConsumer<EntityDeletedEvent<Poll>>,
        //blog posts
        IConsumer<EntityInsertedEvent<BlogPost>>,
        IConsumer<EntityUpdatedEvent<BlogPost>>,
        IConsumer<EntityDeletedEvent<BlogPost>>,
        //blog comments
        IConsumer<EntityDeletedEvent<BlogComment>>,
        //news items
        IConsumer<EntityInsertedEvent<NewsItem>>,
        IConsumer<EntityUpdatedEvent<NewsItem>>,
        IConsumer<EntityDeletedEvent<NewsItem>>,
        //news comments
        IConsumer<EntityDeletedEvent<NewsComment>>,
        //states/province
        IConsumer<EntityInsertedEvent<StateProvince>>,
        IConsumer<EntityUpdatedEvent<StateProvince>>,
        IConsumer<EntityDeletedEvent<StateProvince>>,
        //return requests
        IConsumer<EntityInsertedEvent<ReturnRequestAction>>,
        IConsumer<EntityUpdatedEvent<ReturnRequestAction>>,
        IConsumer<EntityDeletedEvent<ReturnRequestAction>>,
        IConsumer<EntityInsertedEvent<ReturnRequestReason>>,
        IConsumer<EntityUpdatedEvent<ReturnRequestReason>>,
        IConsumer<EntityDeletedEvent<ReturnRequestReason>>,
        //templates
        IConsumer<EntityInsertedEvent<CategoryTemplate>>,
        IConsumer<EntityUpdatedEvent<CategoryTemplate>>,
        IConsumer<EntityDeletedEvent<CategoryTemplate>>,
        IConsumer<EntityInsertedEvent<ManufacturerTemplate>>,
        IConsumer<EntityUpdatedEvent<ManufacturerTemplate>>,
        IConsumer<EntityDeletedEvent<ManufacturerTemplate>>,
        IConsumer<EntityInsertedEvent<ProductTemplate>>,
        IConsumer<EntityUpdatedEvent<ProductTemplate>>,
        IConsumer<EntityDeletedEvent<ProductTemplate>>,
        IConsumer<EntityInsertedEvent<TopicTemplate>>,
        IConsumer<EntityUpdatedEvent<TopicTemplate>>,
        IConsumer<EntityDeletedEvent<TopicTemplate>>,
        //checkout attributes
        IConsumer<EntityInsertedEvent<CheckoutAttribute>>,
        IConsumer<EntityUpdatedEvent<CheckoutAttribute>>,
        IConsumer<EntityDeletedEvent<CheckoutAttribute>>,
        //shopping cart items
        IConsumer<EntityUpdatedEvent<ShoppingCartItem>>,
        //plugins
        IConsumer<PluginUpdatedEvent>
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public ApiCacheEventConsumer(CatalogSettings catalogSettings, IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        //languages
        public async Task HandleEventAsync(EntityInsertedEvent<Language> eventMessage)
        {
            //clear all localizable models
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SearchCategoriesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.TopicPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductBreadcrumbPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductManufacturersPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StateProvincesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.AvailableLanguagesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.AvailableCurrenciesPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Language> eventMessage)
        {
            //clear all localizable models
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SearchCategoriesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.TopicPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductBreadcrumbPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductManufacturersPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StateProvincesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.AvailableLanguagesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.AvailableCurrenciesPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Language> eventMessage)
        {
            //clear all localizable models
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SearchCategoriesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.TopicPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductBreadcrumbPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductManufacturersPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StateProvincesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.AvailableLanguagesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.AvailableCurrenciesPrefixCacheKey);
        }

        //currencies
        public async Task HandleEventAsync(EntityInsertedEvent<Currency> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.AvailableCurrenciesPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Currency> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.AvailableCurrenciesPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Currency> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.AvailableCurrenciesPrefixCacheKey);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Setting> eventMessage)
        {
            //clear models which depend on settings
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTagPopularPrefixCacheKey); //depends on CatalogSettings.NumberOfProductTags
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerNavigationPrefixCacheKey); //depends on CatalogSettings.ManufacturersBlockItemsToDisplay
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.VendorNavigationPrefixCacheKey); //depends on VendorSettings.VendorBlockItemsToDisplay
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryAllPrefixCacheKey); //depends on CatalogSettings.ShowCategoryProductNumber and CatalogSettings.ShowCategoryProductNumberIncludingSubcategories
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryNumberOfProductsPrefixCacheKey); //depends on CatalogSettings.ShowCategoryProductNumberIncludingSubcategories
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.HomepageBestsellersIdsPrefixCacheKey); //depends on CatalogSettings.NumberOfBestsellersOnHomepage
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsAlsoPurchasedIdsPrefixCacheKey); //depends on CatalogSettings.ProductsAlsoPurchasedNumber
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsRelatedIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.BlogPrefixCacheKey); //depends on BlogSettings.NumberOfTags
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.NewsPrefixCacheKey); //depends on NewsSettings.MainPageNewsCount
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey); //depends on distinct sitemap settings
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.WidgetPrefixCacheKey); //depends on WidgetSettings and certain settings of widgets
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StoreLogoPathPrefixCacheKey); //depends on StoreInformationSettings.LogoPictureId
        }

        //vendors
        public async Task HandleEventAsync(EntityInsertedEvent<Vendor> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.VendorNavigationPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Vendor> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.VendorNavigationPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.VendorPicturePrefixCacheKeyById, eventMessage.Entity.Id));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Vendor> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.VendorNavigationPrefixCacheKey);
        }

        //manufacturers
        public async Task HandleEventAsync(EntityInsertedEvent<Manufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);

        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductManufacturersPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ManufacturerPicturePrefixCacheKeyById, eventMessage.Entity.Id));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductManufacturersPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }

        //product manufacturers
        public async Task HandleEventAsync(EntityInsertedEvent<ProductManufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductManufacturersPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ManufacturerHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.ManufacturerId));
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductManufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductManufacturersPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ManufacturerHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.ManufacturerId));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductManufacturer> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductManufacturersPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ManufacturerHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.ManufacturerId));
        }

        //categories
        public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SearchCategoriesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategorySubcategoriesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryHomepagePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SearchCategoriesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductBreadcrumbPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryBreadcrumbPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategorySubcategoriesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryHomepagePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.CategoryPicturePrefixCacheKeyById, eventMessage.Entity.Id));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SearchCategoriesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductBreadcrumbPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryBreadcrumbPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategorySubcategoriesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryHomepagePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }

        //product categories
        public async Task HandleEventAsync(EntityInsertedEvent<ProductCategory> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductBreadcrumbPrefixCacheKeyById, eventMessage.Entity.ProductId));
            if (_catalogSettings.ShowCategoryProductNumber)
            {
                //depends on CatalogSettings.ShowCategoryProductNumber (when enabled)
                //so there's no need to clear this cache in other cases
                await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryAllPrefixCacheKey);
                await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            }
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryNumberOfProductsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.CategoryHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.CategoryId));
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductCategory> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductBreadcrumbPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryNumberOfProductsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.CategoryHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.CategoryId));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductCategory> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductBreadcrumbPrefixCacheKeyById, eventMessage.Entity.ProductId));
            if (_catalogSettings.ShowCategoryProductNumber)
            {
                //depends on CatalogSettings.ShowCategoryProductNumber (when enabled)
                //so there's no need to clear this cache in other cases
                await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryAllPrefixCacheKey);
                await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            }
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryNumberOfProductsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.CategoryHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.CategoryId));
        }

        //products
        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.HomepageBestsellersIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsAlsoPurchasedIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsRelatedIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductReviewsPrefixCacheKeyById, eventMessage.Entity.Id));
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTagByProductPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.HomepageBestsellersIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsAlsoPurchasedIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsRelatedIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }

        //product tags
        public async Task HandleEventAsync(EntityInsertedEvent<ProductTag> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTagPopularPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTagByProductPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTag> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTagPopularPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTagByProductPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductTag> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTagPopularPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTagByProductPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }

        //related products
        public async Task HandleEventAsync(EntityInsertedEvent<RelatedProduct> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsRelatedIdsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<RelatedProduct> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsRelatedIdsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<RelatedProduct> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsRelatedIdsPrefixCacheKey);
        }

        //specification attributes
        public async Task HandleEventAsync(EntityUpdatedEvent<SpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<SpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }

        //specification attribute options
        public async Task HandleEventAsync(EntityUpdatedEvent<SpecificationAttributeOption> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<SpecificationAttributeOption> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }

        //Product specification attribute
        public async Task HandleEventAsync(EntityInsertedEvent<ProductSpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductSpecsPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductSpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductSpecsPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductSpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductSpecsPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }

        //Product attributes
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeValue> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductAttributeImageSquarePicturePrefixCacheKey);
        }

        //Topics
        public async Task HandleEventAsync(EntityInsertedEvent<Topic> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.TopicPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Topic> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.TopicPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Topic> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.TopicPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }

        //Orders
        public async Task HandleEventAsync(EntityInsertedEvent<Order> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.HomepageBestsellersIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsAlsoPurchasedIdsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.HomepageBestsellersIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsAlsoPurchasedIdsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.HomepageBestsellersIdsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductsAlsoPurchasedIdsPrefixCacheKey);
        }

        //Pictures
        public async Task HandleEventAsync(EntityInsertedEvent<Picture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CartPicturePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Picture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CartPicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductDetailsPicturesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductDefaultPicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryPicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerPicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.VendorPicturePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Picture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CartPicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductDetailsPicturesPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductDefaultPicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryPicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerPicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.VendorPicturePrefixCacheKey);
        }

        //Product picture mappings
        public async Task HandleEventAsync(EntityInsertedEvent<ProductPicture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductDefaultPicturePrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductDetailsPicturesPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CartPicturePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductPicture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductDefaultPicturePrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductDetailsPicturesPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CartPicturePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductPicture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductDefaultPicturePrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductDetailsPicturesPrefixCacheKeyById, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CartPicturePrefixCacheKey);
        }

        //Polls
        public async Task HandleEventAsync(EntityInsertedEvent<Poll> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.PollsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Poll> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.PollsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Poll> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.PollsPrefixCacheKey);
        }

        //Blog posts
        public async Task HandleEventAsync(EntityInsertedEvent<BlogPost> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.BlogPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<BlogPost> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.BlogPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<BlogPost> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.BlogPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }

        //Blog comments
        public async Task HandleEventAsync(EntityDeletedEvent<BlogComment> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.BlogCommentsPrefixCacheKey);
        }

        //News items
        public async Task HandleEventAsync(EntityInsertedEvent<NewsItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.NewsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<NewsItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.NewsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<NewsItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.NewsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SitemapPrefixCacheKey);
        }
        //News comments
        public async Task HandleEventAsync(EntityDeletedEvent<NewsComment> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.NewsCommentsPrefixCacheKey);
        }

        //State/province
        public async Task HandleEventAsync(EntityInsertedEvent<StateProvince> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StateProvincesPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<StateProvince> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StateProvincesPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<StateProvince> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StateProvincesPrefixCacheKey);
        }

        //return requests
        public async Task HandleEventAsync(EntityInsertedEvent<ReturnRequestAction> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ReturnRequestActionsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ReturnRequestAction> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ReturnRequestActionsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ReturnRequestAction> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ReturnRequestActionsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityInsertedEvent<ReturnRequestReason> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ReturnRequestReasonsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ReturnRequestReason> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ReturnRequestReasonsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ReturnRequestReason> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ReturnRequestReasonsPrefixCacheKey);
        }

        //templates
        public async Task HandleEventAsync(EntityInsertedEvent<CategoryTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<CategoryTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<CategoryTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CategoryTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityInsertedEvent<ManufacturerTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ManufacturerTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ManufacturerTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ManufacturerTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityInsertedEvent<ProductTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.ProductTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityInsertedEvent<TopicTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.TopicTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<TopicTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.TopicTemplatePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<TopicTemplate> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.TopicTemplatePrefixCacheKey);
        }

        //checkout attributes
        public async Task HandleEventAsync(EntityInsertedEvent<CheckoutAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CheckoutAttributesPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<CheckoutAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CheckoutAttributesPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<CheckoutAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CheckoutAttributesPrefixCacheKey);
        }

        //shopping cart items
        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.CartPicturePrefixCacheKey);
        }

        //product reviews
        public async Task HandleEventAsync(EntityDeletedEvent<ProductReview> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(ApiModelCacheDefaults.ProductReviewsPrefixCacheKeyById, eventMessage.Entity.ProductId));
        }

        /// <summary>
        /// Handle plugin updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public async Task HandleEventAsync(PluginUpdatedEvent eventMessage)
        {
            if (eventMessage?.Plugin?.Instance<IWidgetPlugin>() != null)
                await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.WidgetPrefixCacheKey);
        }

        #endregion
    }
}