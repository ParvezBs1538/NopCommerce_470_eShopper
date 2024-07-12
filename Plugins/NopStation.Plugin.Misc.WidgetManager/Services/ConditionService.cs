using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

namespace NopStation.Plugin.Misc.WidgetManager.Services;

public class ConditionService : IConditionService
{
    #region Fields

    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IRepository<CustomerConditionMapping> _customerConditionMappingRepository;
    private readonly IRepository<ProductConditionMapping> _productConditionMappingRepository;

    #endregion

    #region Ctor

    public ConditionService(IStaticCacheManager staticCacheManager,
        IRepository<CustomerConditionMapping> customerConditionMappingRepository,
        IRepository<ProductConditionMapping> productConditionMappingRepository)
    {
        _staticCacheManager = staticCacheManager;
        _customerConditionMappingRepository = customerConditionMappingRepository;
        _productConditionMappingRepository = productConditionMappingRepository;
    }

    #endregion

    #region Utilities

    protected virtual async Task<bool> IsEntityCustomerConditionMappingExistsAsync<TEntity>(TEntity entity = null)
        where TEntity : BaseEntity, ICustomerConditionSupported
    {
        var entityId = entity?.Id ?? 0;

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.CustomerConditionMappingExistsCacheKey, entityId, entityName);

        var query = from ccm in _customerConditionMappingRepository.Table
                    where ccm.EntityName == entityName && (entityId == 0 || ccm.EntityId == entityId)
                    select ccm.CustomerId;

        return await _staticCacheManager.GetAsync(key, query.Any);
    }

    protected virtual async Task<bool> IsEntityProductConditionMappingExistsAsync<TEntity>(TEntity entity = null)
        where TEntity : BaseEntity, IProductConditionSupported
    {
        var entityId = entity?.Id ?? 0;

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.ProductConditionMappingExistsCacheKey, entityId, entityName);

        var query = from pcm in _productConditionMappingRepository.Table
                    where pcm.EntityName == entityName && (entityId == 0 || pcm.EntityId == entityId)
                    select pcm.ProductId;

        return await _staticCacheManager.GetAsync(key, query.Any);
    }

    #endregion

    #region Methods

    #region Customer condition mappings

    public async Task InsertCustomerConditionMappingAsync<TEntity>(TEntity entity, int customerId) where TEntity : BaseEntity, ICustomerConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (customerId == 0)
            throw new ArgumentOutOfRangeException(nameof(customerId));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        var customerConditionMapping = new CustomerConditionMapping
        {
            EntityId = entityId,
            EntityName = entityName,
            CustomerId = customerId
        };

        await InsertCustomerConditionMappingAsync(customerConditionMapping);
    }

    public virtual async Task InsertCustomerConditionMappingAsync(CustomerConditionMapping customerConditionMapping)
    {
        await _customerConditionMappingRepository.InsertAsync(customerConditionMapping);
    }

    public virtual async Task UpdateCustomerConditionMappingAsync(CustomerConditionMapping customerConditionMapping)
    {
        await _customerConditionMappingRepository.UpdateAsync(customerConditionMapping);
    }

    public async Task<CustomerConditionMapping> GetCustomerConditionMappingByIdAsync(int id)
    {
        return await _customerConditionMappingRepository.GetByIdAsync(id);
    }

    public async Task<CustomerConditionMapping> GetCustomerConditionAppliedToEntityAsync<TEntity>(TEntity entity, int customerId)
        where TEntity : BaseEntity, ICustomerConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        return await GetCustomerConditionAppliedToEntityAsync(entityId, entityName, customerId);
    }

    public async Task<CustomerConditionMapping> GetCustomerConditionAppliedToEntityAsync(int entityId, string entityName, int customerId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.EntityCustomerConditionMappingCacheKey, entityId, entityName, customerId);

        return await _staticCacheManager.GetAsync(key, async () => await _customerConditionMappingRepository.Table
            .FirstOrDefaultAsync(ccm => ccm.EntityName == entityName && ccm.EntityId == entityId && ccm.CustomerId == customerId));
    }

    public async Task DeleteCustomerConditionMappingAsync(CustomerConditionMapping customerConditionMapping)
    {
        await _customerConditionMappingRepository.DeleteAsync(customerConditionMapping);
    }

    public async Task DeleteCustomerConditionMappingsAsync(IList<CustomerConditionMapping> customerConditionMappings)
    {
        await _customerConditionMappingRepository.DeleteAsync(customerConditionMappings);
    }

    public virtual async Task<IQueryable<TEntity>> ApplyCustomerConditionMappingAsync<TEntity>(IQueryable<TEntity> query, int customerId)
        where TEntity : BaseEntity, ICustomerConditionSupported
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        if (customerId == 0 || !await IsEntityCustomerConditionMappingExistsAsync<TEntity>())
            return query;

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        query = from entity in query
                where !entity.HasCustomerConditionApplied || _customerConditionMappingRepository.Table.Any(ccm =>
                    ccm.EntityName == entityName && ccm.EntityId == entity.Id && ccm.CustomerId == customerId)
                select entity;

        return query;
    }

    public virtual async Task UpdateHasCustomerConditionsPropertyAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.HasCustomerConditionApplied = await IsEntityCustomerConditionMappingExistsAsync(entity);

        await EngineContext.Current.Resolve<IRepository<TEntity>>().UpdateAsync(entity);
    }

    public async Task<IList<CustomerConditionMapping>> GetEntityCustomerConditionsAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        return await GetEntityCustomerConditionsAsync(entityId, entityName);
    }

    public async Task<IList<CustomerConditionMapping>> GetEntityCustomerConditionsAsync(int entityId, string entityName)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.EntityCustomerConditionMappingsCacheKey, entityId, entityName);

        var query = from ccm in _customerConditionMappingRepository.Table
                    where ccm.EntityId == entityId &&
                        ccm.EntityName == entityName
                    orderby ccm.Id
                    select ccm;

        return await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());
    }

    public async Task<int[]> GetCustomerConditionsWithAccessAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        return await GetCustomerConditionsWithAccessAsync(entityId, entityName);
    }

    public async Task<int[]> GetCustomerConditionsWithAccessAsync(int entityId, string entityName)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.EntityCustomerConditionsCacheKey, entityId, entityName);

        var query = from ccm in _customerConditionMappingRepository.Table
                    where ccm.EntityId == entityId &&
                          ccm.EntityName == entityName
                    select ccm.CustomerId;

        return await _staticCacheManager.GetAsync(key, () => query.ToArray());
    }

    #endregion

    #region Product condition mappings

    public async Task InsertProductConditionMappingAsync<TEntity>(TEntity entity, int productId) where TEntity : BaseEntity, IProductConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (productId == 0)
            throw new ArgumentOutOfRangeException(nameof(productId));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        var productConditionMapping = new ProductConditionMapping
        {
            EntityId = entityId,
            EntityName = entityName,
            ProductId = productId
        };

        await InsertProductConditionMappingAsync(productConditionMapping);
    }

    public virtual async Task InsertProductConditionMappingAsync(ProductConditionMapping productConditionMapping)
    {
        await _productConditionMappingRepository.InsertAsync(productConditionMapping);
    }

    public virtual async Task UpdateProductConditionMappingAsync(ProductConditionMapping productConditionMapping)
    {
        await _productConditionMappingRepository.UpdateAsync(productConditionMapping);
    }

    public async Task<ProductConditionMapping> GetProductConditionMappingByIdAsync(int id)
    {
        return await _productConditionMappingRepository.GetByIdAsync(id);
    }

    public async Task<ProductConditionMapping> GetProductConditionAppliedToEntityAsync<TEntity>(TEntity entity, int productId)
        where TEntity : BaseEntity, IProductConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        return await GetProductConditionAppliedToEntityAsync(entityId, entityName, productId);
    }

    public async Task<ProductConditionMapping> GetProductConditionAppliedToEntityAsync(int entityId, string entityName, int productId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.EntityProductConditionMappingCacheKey, entityId, entityName, productId);

        return await _staticCacheManager.GetAsync(key, async () => await _productConditionMappingRepository.Table
            .FirstOrDefaultAsync(ccm => ccm.EntityName == entityName && ccm.EntityId == entityId && ccm.ProductId == productId));
    }

    public async Task DeleteProductConditionMappingAsync(ProductConditionMapping productConditionMapping)
    {
        await _productConditionMappingRepository.DeleteAsync(productConditionMapping);
    }

    public async Task DeleteProductConditionMappingsAsync(IList<ProductConditionMapping> productConditionMappings)
    {
        await _productConditionMappingRepository.DeleteAsync(productConditionMappings);
    }

    public virtual async Task<IQueryable<TEntity>> ApplyProductConditionMappingAsync<TEntity>(IQueryable<TEntity> query, int productId)
        where TEntity : BaseEntity, IProductConditionSupported
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        if (!await IsEntityProductConditionMappingExistsAsync<TEntity>())
            return query;

        if (productId > 0)
        {
            var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

            query = from entity in query
                    where !entity.HasProductConditionApplied || _productConditionMappingRepository.Table.Any(pcm =>
                        pcm.EntityName == entityName && pcm.EntityId == entity.Id && pcm.ProductId == productId)
                    select entity;
        }
        else
        {
            query = from entity in query
                    where !entity.HasProductConditionApplied
                    select entity;
        }

        return query;
    }

    public virtual async Task UpdateHasProductConditionsPropertyAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.HasProductConditionApplied = await IsEntityProductConditionMappingExistsAsync(entity);

        await EngineContext.Current.Resolve<IRepository<TEntity>>().UpdateAsync(entity);
    }

    public async Task<IList<ProductConditionMapping>> GetEntityProductConditionsAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        return await GetEntityProductConditionsAsync(entityId, entityName);
    }

    public async Task<IList<ProductConditionMapping>> GetEntityProductConditionsAsync(int entityId, string entityName)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.EntityProductConditionMappingsCacheKey, entityId, entityName);

        var query = from ccm in _productConditionMappingRepository.Table
                    where ccm.EntityId == entityId &&
                        ccm.EntityName == entityName
                    orderby ccm.Id
                    select ccm;

        return await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());
    }

    public async Task<int[]> GetProductConditionsWithAccessAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        return await GetProductConditionsWithAccessAsync(entityId, entityName);
    }

    public async Task<int[]> GetProductConditionsWithAccessAsync(int entityId, string entityName)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.EntityProductConditionsCacheKey, entityId, entityName);

        var query = from ccm in _productConditionMappingRepository.Table
                    where ccm.EntityId == entityId &&
                          ccm.EntityName == entityName
                    select ccm.ProductId;

        return await _staticCacheManager.GetAsync(key, () => query.ToArray());
    }

    #endregion

    #endregion
}
