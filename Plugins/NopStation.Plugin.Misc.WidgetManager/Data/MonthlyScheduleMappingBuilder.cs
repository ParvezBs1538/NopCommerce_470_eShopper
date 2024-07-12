using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.WidgetManager.Domain.Schedules;

namespace NopStation.Plugin.Misc.WidgetManager.Data;

public class MonthlyScheduleMappingBuilder : NopEntityBuilder<MonthlyScheduleMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(MonthlyScheduleMapping.EntityName)).AsString(1024).NotNullable()
            .WithColumn(nameof(MonthlyScheduleMapping.EntityId)).AsInt32();
    }
}
