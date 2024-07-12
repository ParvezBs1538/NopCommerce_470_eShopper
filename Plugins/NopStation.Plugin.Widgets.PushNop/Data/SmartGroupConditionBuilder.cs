using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Data
{
    public class SmartGroupConditionBuilder : NopEntityBuilder<SmartGroupCondition>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
               .WithColumn(nameof(SmartGroupCondition.SmartGroupId)).AsInt32().ForeignKey<SmartGroup>();
        }
    }
}