using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Misc.WidgetManager.Services.Caching;

public partial class WidgetZoneMappingCacheEventConsumer : CacheEventConsumer<WidgetZoneMapping>
{
    protected override async Task ClearCacheAsync(WidgetZoneMapping entity)
    {
        await RemoveAsync(WidgetManagerDefaults.DomainWidgetZonesCacheKey, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.EntityWidgetZonesCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.WidgetZoneMappingsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.EntityWidgetZoneMappingsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.WidgetZoneMappingExistsCacheKey, entity.EntityId, entity.EntityName);
    }
}
