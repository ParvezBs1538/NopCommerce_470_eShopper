using Nop.Core;

namespace NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

public class WeeklyScheduleMapping : BaseEntity
{
    public string EntityName { get; set; }

    public int EntityId { get; set; }

    public int DayId { get; set; }
}
