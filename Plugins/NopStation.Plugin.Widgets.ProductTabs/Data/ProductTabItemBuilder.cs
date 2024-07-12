using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Data
{
    public class ProductTabItemBuilder : NopEntityBuilder<ProductTabItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
              .WithColumn(nameof(ProductTabItem.ProductTabId)).AsInt32().ForeignKey<ProductTab>().NotNullable();
        }
    }
}
