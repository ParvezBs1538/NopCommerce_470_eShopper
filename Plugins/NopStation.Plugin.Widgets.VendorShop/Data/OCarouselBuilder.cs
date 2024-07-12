using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Data
{
    public class OCarouselBuilder : NopEntityBuilder<OCarousel>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(OCarousel.Name))
                .AsString(400)
                .WithColumn(nameof(OCarousel.VendorId))
                .AsInt32()
                .ForeignKey<Vendor>()
                .OnDelete(System.Data.Rule.Cascade);
        }
    }
}