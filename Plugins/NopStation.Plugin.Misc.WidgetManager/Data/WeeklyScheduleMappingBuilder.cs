using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

namespace NopStation.Plugin.Misc.WidgetManager.Data;

public class WeeklyScheduleMappingBuilder : NopEntityBuilder<WeeklyScheduleMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(WeeklyScheduleMapping.EntityName)).AsString(1024).NotNullable()
            .WithColumn(nameof(WeeklyScheduleMapping.EntityId)).AsInt32();
    }
}
