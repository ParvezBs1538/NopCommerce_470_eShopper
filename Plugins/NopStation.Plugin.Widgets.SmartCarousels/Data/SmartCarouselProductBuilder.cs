using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Data;

public class SmartCarouselProductBuilder : NopEntityBuilder<SmartCarouselProductMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(SmartCarouselProductMapping.CarouselId)).AsInt32().ForeignKey<SmartCarousel>()
             .WithColumn(nameof(SmartCarouselProductMapping.ProductId)).AsInt32().ForeignKey<Product>();
    }
}
