using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Data
{
    public class OCarouselItemBuilder : NopEntityBuilder<OCarouselItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(OCarouselItem.OCarouselId)).AsInt32().ForeignKey<OCarousel>();
        }
    }
}
