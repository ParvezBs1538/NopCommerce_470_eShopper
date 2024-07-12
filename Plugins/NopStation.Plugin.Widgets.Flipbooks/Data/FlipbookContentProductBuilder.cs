using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.Flipbooks.Domains;

namespace NopStation.Plugin.Widgets.Flipbooks.Data
{
    public class FlipbookContentProductBuilder : NopEntityBuilder<FlipbookContentProduct>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FlipbookContentProduct.FlipbookContentId)).AsInt32().ForeignKey<FlipbookContent>()
                .WithColumn(nameof(FlipbookContentProduct.ProductId)).AsInt32().ForeignKey<Product>()
                .WithColumn(nameof(FlipbookContentProduct.DisplayOrder)).AsInt32();
        }
    }
}
