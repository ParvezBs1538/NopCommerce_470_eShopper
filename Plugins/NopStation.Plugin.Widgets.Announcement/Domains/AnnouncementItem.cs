using System;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

namespace NopStation.Plugin.Widgets.Announcement.Domains;

public partial class AnnouncementItem : BaseEntity, ILocalizedEntity, IStoreMappingSupported, IAclSupported,
    IScheduleSupported, ICustomerConditionSupported
{
    public int DisplayOrder { get; set; }

    public string Name { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string Color { get; set; }

    public bool Active { get; set; }

    public bool LimitedToStores { get; set; }

    public bool SubjectToAcl { get; set; }


    public DateTime? AvaliableDateTimeFromUtc { get; set; }

    public DateTime? AvaliableDateTimeToUtc { get; set; }

    public int ScheduleTypeId { get; set; }

    public long? TimeOfDayFromTicksUtc { get; set; }

    public long? TimeOfDayToTicksUtc { get; set; }

    public bool HasCustomerConditionApplied { get; set; }

    public ScheduleType ScheduleType
    {
        get => (ScheduleType)ScheduleTypeId;
        set => ScheduleTypeId = (int)value;
    }
}
