using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

namespace NopStation.Plugin.Misc.WidgetManager.Services.Caching;

public partial class WeeklyScheduleMappingCacheEventConsumer : CacheEventConsumer<WeeklyScheduleMapping>
{
    protected override async Task ClearCacheAsync(WeeklyScheduleMapping entity)
    {
        await RemoveAsync(WidgetManagerDefaults.ScheduleWeeklyMappingsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.ScheduleWeeklyDaysCacheKey, entity.EntityId, entity.EntityName);
    }
}
