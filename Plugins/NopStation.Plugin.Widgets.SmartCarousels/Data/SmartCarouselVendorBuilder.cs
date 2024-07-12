using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Data;

public class SmartCarouselVendorBuilder : NopEntityBuilder<SmartCarouselVendorMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(SmartCarouselVendorMapping.CarouselId)).AsInt32().ForeignKey<SmartCarousel>()
             .WithColumn(nameof(SmartCarouselVendorMapping.VendorId)).AsInt32().ForeignKey<Vendor>();
    }
}
