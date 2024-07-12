using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Misc.WidgetManager.Data;

public class WidgetZoneMappingBuilder : NopEntityBuilder<WidgetZoneMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(WidgetZoneMapping.WidgetZone)).AsString(1024).NotNullable()
            .WithColumn(nameof(WidgetZoneMapping.EntityName)).AsString(1024).NotNullable()
            .WithColumn(nameof(WidgetZoneMapping.EntityId)).AsInt32();
    }
}
