using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Misc.WidgetManager.Services;

public interface IWidgetZoneService
{
    Task InsertWidgetZoneMappingAsync<TEntity>(TEntity entity, string widgetZone, int displayOrder = 0)
        where TEntity : BaseEntity, IWidgetZoneSupported;

    Task InsertWidgetZoneMappingAsync(WidgetZoneMapping widgetZoneMapping);

    Task UpdateWidgetZoneMappingAsync(WidgetZoneMapping widgetZoneMapping);

    Task<WidgetZoneMapping> GetWidgetZoneMappingByIdAsync(int id);

    Task DeleteWidgetZoneMappingAsync(WidgetZoneMapping widgetZoneMapping);

    Task DeleteWidgetZoneMappingsAsync(IList<WidgetZoneMapping> widgetZoneMappings);

    Task<IQueryable<TEntity>> ApplyWidgetZoneMappingAsync<TEntity>(IQueryable<TEntity> query, string widgetZone)
       where TEntity : BaseEntity, IWidgetZoneSupported;

    Task UpdateHasWidgetZonesPropertyAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported;

    Task<IList<WidgetZoneMapping>> GetEntityWidgetZoneMappingsAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported;

    Task<IList<WidgetZoneMapping>> GetEntityWidgetZoneMappingsAsync(int entityId, string entityName);

    Task<string[]> GetWidgetZonesWithAccessAsync<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported;

    Task<string[]> GetWidgetZonesWithAccessAsync(int entityId, string entityName);

    Task<string[]> GetWidgetZonesForDomainAsync<TEntity>();

    Task<WidgetZoneMapping> GetWidgetZoneAppliedToEntityAsync<TEntity>(TEntity entity, string widgetZone)
        where TEntity : BaseEntity, IWidgetZoneSupported;

    Task<WidgetZoneMapping> GetWidgetZoneAppliedToEntityAsync(int entityId, string entityName, string widgetZone);
}
