using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.IpFilter.Domain;

namespace NopStation.Plugin.Misc.IpFilter.Data
{
    public class IpBlockRuleBuilder : NopEntityBuilder<IpBlockRule>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(IpBlockRule.IpAddress)).AsString(30).NotNullable();
        }
    }
}
