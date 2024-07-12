using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Data;

public class SmartCarouselManufacturerBuilder : NopEntityBuilder<SmartCarouselManufacturerMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(SmartCarouselManufacturerMapping.CarouselId)).AsInt32().ForeignKey<SmartCarousel>()
             .WithColumn(nameof(SmartCarouselManufacturerMapping.ManufacturerId)).AsInt32().ForeignKey<Manufacturer>();
    }
}
