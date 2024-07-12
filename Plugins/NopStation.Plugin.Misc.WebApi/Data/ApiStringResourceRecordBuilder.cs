using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Data
{
    public class ApiStringResourceRecordBuilder : NopEntityBuilder<ApiStringResource>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ApiStringResource.ResourceName))
                .AsString().Nullable();
        }
    }
}
