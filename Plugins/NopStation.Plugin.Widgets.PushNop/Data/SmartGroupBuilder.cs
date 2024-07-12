using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Data
{
    public class SmartGroupBuilder : NopEntityBuilder<SmartGroup>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
               .WithColumn(nameof(SmartGroup.Name)).AsString(200).NotNullable();
        }
    }
}
