using Nop.Core;

namespace NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

public class ConditionGroupMapping : BaseEntity
{
    public int EntityId { get; set; }

    public string EntityName { get; set; }
}
