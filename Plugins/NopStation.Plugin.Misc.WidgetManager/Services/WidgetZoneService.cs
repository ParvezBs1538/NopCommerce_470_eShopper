using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Misc.WidgetManager.Services;

public class WidgetZoneService : IWidgetZoneService
{
    #region Fields

    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IRepository<WidgetZoneMapping> _widgetZoneMappingRepository;

    #endregion

    #region Ctor

    public WidgetZoneService(IStaticCacheManager staticCacheManager,
        IRepository<WidgetZoneMapping> widgetZoneMappingRepository)
    {
        _staticCacheManager = staticCacheManager;
        _widgetZoneMappingRepository = widgetZoneMappingRepository;
    }

    #endregion

    #region Utilities

    protected virtual async Task<bool> IsEntityMappingExistsAsync<TEntity>(TEntity entity) where TEntity : BaseEntity, IWidgetZoneSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.WidgetZoneMappingExistsCacheKey, entity.Id, entityName);

        var query = from wzm in _widgetZoneMappingRepository.Table
                    where wzm.EntityName == entityName && wzm.EntityId == entity.Id
                    select wzm.WidgetZone;

        return await _staticCacheManager.GetAsync(key, query.Any);
    }

    #endregion

    #region Methods

    public async Task InsertWidgetZoneMappingAsync<TEntity>(TEntity entity, string widgetZone, int displayOrder = 0) where TEntity : BaseEntity, IWidgetZoneSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrWhiteSpace(widgetZone))
            throw new ArgumentOutOfRangeException(nameof(widgetZone));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        var widgetZoneMapping = new WidgetZoneMapping
        {
            EntityId = entityId,
            EntityName = entityName,
            WidgetZone = widgetZone,
            DisplayOrder = displayOrder
        };

        await InsertWidgetZoneMappingAsync(widgetZoneMapping);
    }

    public virtual async Task InsertWidgetZoneMappingAsync(WidgetZoneMapping widgetZoneMapping)
    {
        await _widgetZoneMappingRepository.InsertAsync(widgetZoneMapping);
    }

    public virtual async Task UpdateWidgetZoneMappingAsync(WidgetZoneMapping widgetZoneMapping)
    {
        await _widgetZoneMappingRepository.UpdateAsync(widgetZoneMapping);
    }

    public async Task<WidgetZoneMapping> GetWidgetZoneMappingByIdAsync(int id)
    {
        return await _widgetZoneMappingRepository.GetByIdAsync(id);
    }

    public async Task DeleteWidgetZoneMappingAsync(WidgetZoneMapping widgetZoneMapping)
    {
        await _widgetZoneMappingRepository.DeleteAsync(widgetZoneMapping);
    }

    public async Task DeleteWidgetZoneMappingsAsync(IList<WidgetZoneMapping> widgetZoneMappings)
    {
        await _widgetZoneMappingRepository.DeleteAsync(widgetZoneMappings);
    }

    public virtual Task<IQueryable<TEntity>> ApplyWidgetZoneMappingAsync<TEntity>(IQueryable<TEntity> query, string widgetZone)
        where TEntity : BaseEntity, IWidgetZoneSupported
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        if (string.IsNullOrWhiteSpace(widgetZone))
            return Task.FromResult(query);

        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

        query = from entity in query
                where entity.HasWidgetZoneMappingApplied && _widgetZoneMappingRepository.Table.Any(wzm =>
                      wzm.EntityName == entityName && wzm.EntityId == entity.Id && wzm.WidgetZone == widgetZone)
                select entity;

        return Task.FromResult(query);
    }

    public virtual async Task UpdateHasWidgetZonesPropertyAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.HasWidgetZoneMappingApplied = await IsEntityMappingExistsAsync(entity);

        await EngineContext.Current.Resolve<IRepository<TEntity>>().UpdateAsync(entity);
    }

    public async Task<IList<WidgetZoneMapping>> GetEntityWidgetZoneMappingsAsync<TEntity>(TEntity entity) where TEntity : BaseEntity, IWidgetZoneSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        return await GetEntityWidgetZoneMappingsAsync(entityId, entityName);
    }

    public async Task<IList<WidgetZoneMapping>> GetEntityWidgetZoneMappingsAsync(int entityId, string entityName)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.EntityWidgetZoneMappingsCacheKey, entityId, entityName);

        var query = from wzm in _widgetZoneMappingRepository.Table
                    where wzm.EntityId == entityId &&
                        wzm.EntityName == entityName
                    orderby wzm.DisplayOrder
                    select wzm;

        return await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());
    }

    public async Task<string[]> GetWidgetZonesWithAccessAsync<TEntity>(TEntity entity) where TEntity : BaseEntity, IWidgetZoneSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        return await GetWidgetZonesWithAccessAsync(entityId, entityName);
    }

    public async Task<string[]> GetWidgetZonesWithAccessAsync(int entityId, string entityName)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.EntityWidgetZonesCacheKey, entityId, entityName);

        var query = from wzm in _widgetZoneMappingRepository.Table
                    where (entityId == 0 || wzm.EntityId == entityId) &&
                          wzm.EntityName == entityName
                    select wzm.WidgetZone;

        return await _staticCacheManager.GetAsync(key, () => query.ToArray());
    }

    public async Task<string[]> GetWidgetZonesForDomainAsync<TEntity>()
    {
        var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.DomainWidgetZonesCacheKey, entityName);

        var query = from wzm in _widgetZoneMappingRepository.Table
                    where wzm.EntityName == entityName
                    select wzm.WidgetZone;

        return await _staticCacheManager.GetAsync(key, () => query.ToArray());
    }

    public async Task<WidgetZoneMapping> GetWidgetZoneAppliedToEntityAsync<TEntity>(TEntity entity, string widgetZone)
        where TEntity : BaseEntity, IWidgetZoneSupported
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityId = entity.Id;
        var entityName = NameCompatibilityManager.GetTableName(entity.GetType());

        return await GetWidgetZoneAppliedToEntityAsync(entityId, entityName, widgetZone);
    }

    public async Task<WidgetZoneMapping> GetWidgetZoneAppliedToEntityAsync(int entityId, string entityName, string widgetZone)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(WidgetManagerDefaults.EntityWidgetZoneMappingCacheKey, entityId, entityName, widgetZone);

        return await _staticCacheManager.GetAsync(key, async () => await _widgetZoneMappingRepository.Table
            .FirstOrDefaultAsync(ccm => ccm.EntityName == entityName && ccm.EntityId == entityId && ccm.WidgetZone == widgetZone));
    }

    #endregion
}
