using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;

namespace NopStation.Plugin.Widgets.AffiliateStation.Data
{
    public class CatalogCommissionRecordBuilder : NopEntityBuilder<CatalogCommission>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CatalogCommission.EntityName)).AsString(400);
        }
    }
}