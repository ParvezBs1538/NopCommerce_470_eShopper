using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

namespace NopStation.Plugin.Misc.WidgetManager.Services.Caching;

public partial class MonthlyScheduleMappingCacheEventConsumer : CacheEventConsumer<MonthlyScheduleMapping>
{
    protected override async Task ClearCacheAsync(MonthlyScheduleMapping entity)
    {
        await RemoveAsync(WidgetManagerDefaults.ScheduleMonthlyMappingsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.ScheduleMonthlyDaysCacheKey, entity.EntityId, entity.EntityName);
    }
}
