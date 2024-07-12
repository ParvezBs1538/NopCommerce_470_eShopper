using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.StoreLocator.Domain;

namespace NopStation.Plugin.Widgets.StoreLocator.Data
{
    public class StoreLocationMap : NopEntityBuilder<StoreLocation>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(StoreLocation.Name)).AsString().NotNullable();
        }
    }
}
