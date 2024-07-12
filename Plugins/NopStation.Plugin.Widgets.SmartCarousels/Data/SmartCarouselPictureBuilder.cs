using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Media;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Data;

public class SmartCarouselPictureBuilder : NopEntityBuilder<SmartCarouselPictureMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(SmartCarouselPictureMapping.CarouselId)).AsInt32().ForeignKey<SmartCarousel>()
             .WithColumn(nameof(SmartCarouselPictureMapping.PictureId)).AsInt32().ForeignKey<Picture>();
        ;
    }
}
