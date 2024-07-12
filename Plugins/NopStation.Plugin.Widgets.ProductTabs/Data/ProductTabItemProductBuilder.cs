using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Data
{
    public class ProductTabItemProductBuilder : NopEntityBuilder<ProductTabItemProduct>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
              .WithColumn(nameof(ProductTabItemProduct.ProductTabItemId)).AsInt32().ForeignKey<ProductTabItem>().NotNullable();
        }
    }
}
