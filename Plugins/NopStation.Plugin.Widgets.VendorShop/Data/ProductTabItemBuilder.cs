using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Data
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
