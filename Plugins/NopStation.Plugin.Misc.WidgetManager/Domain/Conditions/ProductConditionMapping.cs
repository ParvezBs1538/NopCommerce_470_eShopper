using Nop.Core;

namespace NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

public class ProductConditionMapping : BaseEntity
{
    public int EntityId { get; set; }

    public string EntityName { get; set; }

    public int ProductId { get; set; }
}
