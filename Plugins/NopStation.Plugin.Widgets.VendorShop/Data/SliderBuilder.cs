using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Data
{
    public class SliderBuilder : NopEntityBuilder<Slider>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Slider.Name))
                .AsString(400)
                .NotNullable()
                .WithColumn(nameof(Slider.VendorId))
                .AsInt32()
                .ForeignKey<Vendor>()
                .OnDelete(System.Data.Rule.Cascade);
        }
    }
}