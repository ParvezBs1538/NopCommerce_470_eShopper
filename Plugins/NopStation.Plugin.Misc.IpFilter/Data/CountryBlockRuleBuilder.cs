using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Directory;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.IpFilter.Domain;
using Nop.Data.Extensions;

namespace NopStation.Plugin.Misc.IpFilter.Data
{
    public class CountryBlockRuleBuilder : NopEntityBuilder<CountryBlockRule>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CountryBlockRule.CountryId)).AsInt32().ForeignKey<Country>();
        }
    }
}
