using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Services.Catalog;
using Nop.Services.Events;
using NopStation.Plugin.Misc.AlgoliaSearch.Services;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure
{
    public class AlgoliaEntityEventConsumer : IConsumer<EntityInsertedEvent<Language>>,
        IConsumer<EntityUpdatedEvent<Language>>,
        IConsumer<EntityDeletedEvent<Language>>,
        IConsumer<EntityUpdatedEvent<Picture>>,
        IConsumer<EntityDeletedEvent<Picture>>,
        IConsumer<EntityInsertedEvent<ProductPicture>>,
        IConsumer<EntityUpdatedEvent<ProductPicture>>,
        IConsumer<EntityDeletedEvent<ProductPicture>>,
        IConsumer<EntityInsertedEvent<Product>>,
        IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityDeletedEvent<Product>>,
        IConsumer<EntityUpdatedEvent<Vendor>>,
        IConsumer<EntityDeletedEvent<Vendor>>,
        IConsumer<EntityUpdatedEvent<Manufacturer>>,
        IConsumer<EntityDeletedEvent<Manufacturer>>,
        IConsumer<EntityUpdatedEvent<Category>>,
        IConsumer<EntityDeletedEvent<Category>>,
        IConsumer<EntityInsertedEvent<ProductManufacturer>>,
        IConsumer<EntityUpdatedEvent<ProductManufacturer>>,
        IConsumer<EntityDeletedEvent<ProductManufacturer>>,
        IConsumer<EntityInsertedEvent<ProductCategory>>,
        IConsumer<EntityUpdatedEvent<ProductCategory>>,
        IConsumer<EntityDeletedEvent<ProductCategory>>,
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
        //attributes
        IConsumer<EntityUpdatedEvent<ProductAttribute>>,
        IConsumer<EntityDeletedEvent<ProductAttribute>>,
        //attribute options
        IConsumer<EntityUpdatedEvent<ProductAttributeValue>>,
        IConsumer<EntityDeletedEvent<ProductAttributeValue>>,
        //Product attribute values
        IConsumer<EntityInsertedEvent<ProductAttributeMapping>>,
        IConsumer<EntityUpdatedEvent<ProductAttributeMapping>>,
        IConsumer<EntityDeletedEvent<ProductAttributeMapping>>,
        //Product attribute combination
        IConsumer<EntityInsertedEvent<ProductAttributeCombination>>,
        IConsumer<EntityUpdatedEvent<ProductAttributeCombination>>,
        IConsumer<EntityDeletedEvent<ProductAttributeCombination>>
    {
        #region Fields

        private readonly IStaticCacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly INopDataProvider _dataProvider;
        private readonly IAlgoliaUpdatableItemService _algoliaUpdatableItemService;

        #endregion

        #region Ctor

        public AlgoliaEntityEventConsumer(IStaticCacheManager cacheManager,
            IWorkContext workContext,
            IProductService productService,
            INopDataProvider dataProvider,
            IAlgoliaUpdatableItemService algoliaUpdatableItemService)
        {
            _cacheManager = cacheManager;
            _workContext = workContext;
            _productService = productService;
            _dataProvider = dataProvider;
            _algoliaUpdatableItemService = algoliaUpdatableItemService;
        }

        #endregion

        //languages
        public async Task HandleEventAsync(EntityInsertedEvent<Language> eventMessage)
        {
            //clear all localizable models
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKey);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Language> eventMessage)
        {
            //clear all localizable models
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Language> eventMessage)
        {
            //clear all localizable models
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKey);
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKey);
        }

        //Product picture 
        public async Task HandleEventAsync(EntityUpdatedEvent<Picture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.AutoCompletePicturePrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Picture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.AutoCompletePicturePrefixCacheKey);
        }

        //Product picture mappings
        public async Task HandleEventAsync(EntityInsertedEvent<ProductPicture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(AlgoliaModelCacheDefaults.AutoCompletePicturePicturePrefixCacheKeyById, eventMessage.Entity.ProductId));
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductPicture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(AlgoliaModelCacheDefaults.AutoCompletePicturePicturePrefixCacheKeyById, eventMessage.Entity.ProductId));
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductPicture> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(AlgoliaModelCacheDefaults.AutoCompletePicturePicturePrefixCacheKeyById, eventMessage.Entity.ProductId));
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
        }

        //Product 
        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrscombinationPrefixCacheKey);
            await InsertUpdatableItemAsync(eventMessage.Entity);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrscombinationPrefixCacheKey);
            await InsertUpdatableItemAsync(eventMessage.Entity);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrscombinationPrefixCacheKey);
            await InsertUpdatableItemAsync(eventMessage.Entity);
        }

        //Vendor 
        public async Task HandleEventAsync(EntityUpdatedEvent<Vendor> eventMessage)
        {
            var productCount = await _productService.GetNumberOfProductsByVendorIdAsync(eventMessage.Entity.Id);
            if (productCount != 0)
                await InsertUpdatableItemAsync(eventMessage.Entity);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Vendor> eventMessage)
        {
            var productCount = await _productService.GetNumberOfProductsByVendorIdAsync(eventMessage.Entity.Id);
            if (productCount != 0)
                await InsertUpdatableItemAsync(eventMessage.Entity);
        }
        //Manufacturer 
        public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            var productCount = (await _productService.GetManufacturerFeaturedProductsAsync(eventMessage.Entity.Id)).Count;
            if (productCount != 0)
                await InsertUpdatableItemAsync(eventMessage.Entity);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage)
        {
            var productCount = (await _productService.GetManufacturerFeaturedProductsAsync(eventMessage.Entity.Id)).Count;
            if (productCount != 0)
                await InsertUpdatableItemAsync(eventMessage.Entity);
        }

        //Category 
        public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
        {
            var productCount = await _productService.GetNumberOfProductsInCategoryAsync(new List<int> { eventMessage.Entity.Id });
            if (productCount != 0)
                await InsertUpdatableItemAsync(eventMessage.Entity);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
        {
            var productCount = await _productService.GetNumberOfProductsInCategoryAsync(new List<int> { eventMessage.Entity.Id });
            if (productCount != 0)
                await InsertUpdatableItemAsync(eventMessage.Entity);
            await InsertUpdatableItemAsync(eventMessage.Entity);
        }

        //Product manufacturer 
        public async Task HandleEventAsync(EntityInsertedEvent<ProductManufacturer> eventMessage)
        {
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductManufacturer> eventMessage)
        {
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductManufacturer> eventMessage)
        {
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
        }

        //Product category 
        public async Task HandleEventAsync(EntityInsertedEvent<ProductCategory> eventMessage)
        {
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductCategory> eventMessage)
        {
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductCategory> eventMessage)
        {
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
        }

        //specification attributes
        public async Task HandleEventAsync(EntityUpdatedEvent<SpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<SpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKey);
        }

        //specification attribute options
        public async Task HandleEventAsync(EntityUpdatedEvent<SpecificationAttributeOption> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<SpecificationAttributeOption> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKey);
        }

        //Product specification attribute
        public async Task HandleEventAsync(EntityInsertedEvent<ProductSpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKeyById, eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductSpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKeyById, eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductSpecificationAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(AlgoliaModelCacheDefaults.ProductSpecsPrefixCacheKeyById, eventMessage.Entity.ProductId));
        }

        //attributes
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttribute> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKey);
        }

        //attribute values
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeValue> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeValue> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKey);
        }

        //Product attribute mappings
        public async Task HandleEventAsync(EntityInsertedEvent<ProductAttributeMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKeyById, eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKeyById, eventMessage.Entity.ProductId));
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeMapping> eventMessage)
        {
            await _cacheManager.RemoveByPrefixAsync(string.Format(AlgoliaModelCacheDefaults.ProductAttrsPrefixCacheKeyById, eventMessage.Entity.ProductId));
        }

        //Product attribute combination
        public async Task HandleEventAsync(EntityInsertedEvent<ProductAttributeCombination> eventMessage)
        {
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrscombinationPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<ProductAttributeCombination> eventMessage)
        {
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrscombinationPrefixCacheKey);
        }
        public async Task HandleEventAsync(EntityDeletedEvent<ProductAttributeCombination> eventMessage)
        {
            await InsertUpdatableItemAsync(await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefixAsync(AlgoliaModelCacheDefaults.ProductAttrscombinationPrefixCacheKey);
        }

        public async Task InsertUpdatableItemAsync(BaseEntity entity)
        {
            var entityName = NameCompatibilityManager.GetTableName(entity.GetType());
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            string sql;
            if (DataSettingsManager.LoadSettings().DataProvider == DataProviderType.SqlServer)
            {
                sql = $"IF EXISTS(select * from NS_AlgoliaUpdatableItem where EntityId = {entity.Id} AND EntityName = '{entityName}') " +
                            $"UPDATE NS_AlgoliaUpdatableItem set UpdatedOnUtc = getdate(), LastUpdatedBy = {currentCustomer.Id} where EntityId = {entity.Id} AND EntityName = '{entityName}' " +
                        $"ELSE " +
                            $"INSERT INTO NS_AlgoliaUpdatableItem(EntityId, EntityName, LastUpdatedBy, UpdatedOnUtc) values({entity.Id}, '{entityName}', {currentCustomer.Id}, getdate());";
            }
            else if (DataSettingsManager.LoadSettings().DataProvider == DataProviderType.MySql ||
                DataSettingsManager.LoadSettings().DataProvider == DataProviderType.PostgreSQL)
            {
                sql = $"IF EXISTS(select * from NS_AlgoliaUpdatableItem where EntityId = {entity.Id} AND EntityName = '{entityName}') " +
                            $"UPDATE NS_AlgoliaUpdatableItem set UpdatedOnUtc = NOW(), LastUpdatedBy = {currentCustomer.Id} where EntityId = {entity.Id} AND EntityName = '{entityName}' " +
                        $"ELSE " +
                            $"INSERT INTO NS_AlgoliaUpdatableItem(EntityId, EntityName, LastUpdatedBy, UpdatedOnUtc) values({entity.Id}, '{entityName}', {currentCustomer.Id}, NOW());";
            }
            else
                throw new NotSupportedException();

            await _dataProvider.ExecuteNonQueryAsync(sql);
        }
    }
}
