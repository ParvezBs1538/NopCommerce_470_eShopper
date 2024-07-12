using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Misc.WidgetManager.Data;

public class BaseNameCompatability : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
    {
        {typeof(WidgetZoneMapping),"NS_WM_WidgetZoneMapping" },
        {typeof(MonthlyScheduleMapping),"NS_WM_MonthlyScheduleMapping" },
        {typeof(WeeklyScheduleMapping),"NS_WM_WeeklyScheduleMapping" },
        {typeof(CustomerConditionMapping),"NS_WM_CustomerConditionMapping" },
        {typeof(ProductConditionMapping),"NS_WM_ProductConditionMapping" },
        {typeof(ConditionGroupMapping),"NS_WM_ConditionGroupMapping" },
        {typeof(ConditionRecord),"NS_WM_ConditionRecord" }
    };

    public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
    {
    };
}
