using Nop.Core;

namespace NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

public class CustomerConditionMapping : BaseEntity
{
    public int EntityId { get; set; }

    public string EntityName { get; set; }

    public int CustomerId { get; set; }
}
