using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

namespace NopStation.Plugin.Misc.WidgetManager.Services;

public interface IConditionService
{
    #region Customer condition mappings

    Task InsertCustomerConditionMappingAsync<TEntity>(TEntity entity, int customerId)
        where TEntity : BaseEntity, ICustomerConditionSupported;

    Task InsertCustomerConditionMappingAsync(CustomerConditionMapping customerConditionMapping);

    Task UpdateCustomerConditionMappingAsync(CustomerConditionMapping customerConditionMapping);

    Task<CustomerConditionMapping> GetCustomerConditionMappingByIdAsync(int id);

    Task<CustomerConditionMapping> GetCustomerConditionAppliedToEntityAsync<TEntity>(TEntity entity, int customerId)
       where TEntity : BaseEntity, ICustomerConditionSupported;

    Task<CustomerConditionMapping> GetCustomerConditionAppliedToEntityAsync(int entityId, string entityName, int customerId);

    Task DeleteCustomerConditionMappingAsync(CustomerConditionMapping customerConditionMapping);

    Task DeleteCustomerConditionMappingsAsync(IList<CustomerConditionMapping> customerConditionMappings);

    Task<IQueryable<TEntity>> ApplyCustomerConditionMappingAsync<TEntity>(IQueryable<TEntity> query, int customerId)
        where TEntity : BaseEntity, ICustomerConditionSupported;

    Task UpdateHasCustomerConditionsPropertyAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported;

    Task<IList<CustomerConditionMapping>> GetEntityCustomerConditionsAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported;

    Task<IList<CustomerConditionMapping>> GetEntityCustomerConditionsAsync(int entityId, string entityName);

    Task<int[]> GetCustomerConditionsWithAccessAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported;

    Task<int[]> GetCustomerConditionsWithAccessAsync(int entityId, string entityName);

    #endregion

    #region Product condition mappings

    Task InsertProductConditionMappingAsync<TEntity>(TEntity entity, int productId)
        where TEntity : BaseEntity, IProductConditionSupported;

    Task InsertProductConditionMappingAsync(ProductConditionMapping productConditionMapping);

    Task UpdateProductConditionMappingAsync(ProductConditionMapping productConditionMapping);

    Task<ProductConditionMapping> GetProductConditionMappingByIdAsync(int id);

    Task<ProductConditionMapping> GetProductConditionAppliedToEntityAsync<TEntity>(TEntity entity, int productId)
       where TEntity : BaseEntity, IProductConditionSupported;

    Task<ProductConditionMapping> GetProductConditionAppliedToEntityAsync(int entityId, string entityName, int productId);

    Task DeleteProductConditionMappingAsync(ProductConditionMapping productConditionMapping);

    Task DeleteProductConditionMappingsAsync(IList<ProductConditionMapping> productConditionMappings);

    Task<IQueryable<TEntity>> ApplyProductConditionMappingAsync<TEntity>(IQueryable<TEntity> query, int productId)
        where TEntity : BaseEntity, IProductConditionSupported;

    Task UpdateHasProductConditionsPropertyAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported;

    Task<IList<ProductConditionMapping>> GetEntityProductConditionsAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported;

    Task<IList<ProductConditionMapping>> GetEntityProductConditionsAsync(int entityId, string entityName);

    Task<int[]> GetProductConditionsWithAccessAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported;

    Task<int[]> GetProductConditionsWithAccessAsync(int entityId, string entityName);

    #endregion
}