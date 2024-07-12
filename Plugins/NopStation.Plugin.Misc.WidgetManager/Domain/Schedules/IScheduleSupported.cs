using System;

namespace NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

public interface IScheduleSupported
{
    DateTime? AvaliableDateTimeFromUtc { get; set; }

    DateTime? AvaliableDateTimeToUtc { get; set; }

    int ScheduleTypeId { get; set; }

    long? TimeOfDayFromTicksUtc { get; set; }

    long? TimeOfDayToTicksUtc { get; set; }

    ScheduleType ScheduleType { get; set; }
}
