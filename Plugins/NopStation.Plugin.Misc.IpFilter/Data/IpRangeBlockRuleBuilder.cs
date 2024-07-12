using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.IpFilter.Domain;

namespace NopStation.Plugin.Misc.IpFilter.Data
{
    public class IpRangeBlockRuleBuilder : NopEntityBuilder<IpRangeBlockRule>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(IpRangeBlockRule.FromIpAddress)).AsString(30).NotNullable()
                .WithColumn(nameof(IpRangeBlockRule.ToIpAddress)).AsString(30).NotNullable();
        }
    }
}
