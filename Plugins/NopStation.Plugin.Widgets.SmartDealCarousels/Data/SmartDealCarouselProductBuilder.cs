using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Data;

public class SmartDealCarouselProductBuilder : NopEntityBuilder<SmartDealCarouselProductMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(SmartDealCarouselProductMapping.CarouselId)).AsInt32().ForeignKey<SmartDealCarousel>()
             .WithColumn(nameof(SmartDealCarouselProductMapping.ProductId)).AsInt32().ForeignKey<Product>();
    }
}
