using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Data
{
    public class VendorFeatureMappingBuilder : NopEntityBuilder<VendorFeatureMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(VendorFeatureMapping.Enable))
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false)
                .WithColumn(nameof(VendorFeatureMapping.VendorId))
                .AsInt32()
                .ForeignKey<Vendor>()
                .OnDelete(System.Data.Rule.Cascade);
        }
    }
}