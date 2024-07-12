using Nop.Core;

namespace NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

public class WidgetZoneMapping : BaseEntity
{
    public string EntityName { get; set; }

    public int EntityId { get; set; }

    public string WidgetZone { get; set; }

    public int DisplayOrder { get; set; }
}
