using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Data
{
    public class ProductTabBuilder : NopEntityBuilder<ProductTab>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(ProductTab.Name))
                 .AsString(400)
                 .WithColumn(nameof(ProductTab.VendorId))
                .AsInt32()
                .ForeignKey<Vendor>()
                .OnDelete(System.Data.Rule.Cascade);
        }
    }
}
