using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Data
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
