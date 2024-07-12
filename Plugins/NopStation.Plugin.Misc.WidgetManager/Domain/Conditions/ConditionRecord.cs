using Nop.Core;

namespace NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

public class ConditionRecord : BaseEntity
{
    public int ConditionGroupId { get; set; }

    public int ConditionTypeId { get; set; }

    public int ConditionPropertyId { get; set; }

    public int OperatorId { get; set; }

    public string ConditionValue { get; set; }
}
