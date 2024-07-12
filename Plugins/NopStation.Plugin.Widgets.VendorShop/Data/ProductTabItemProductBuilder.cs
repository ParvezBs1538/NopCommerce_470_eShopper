using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Data
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
