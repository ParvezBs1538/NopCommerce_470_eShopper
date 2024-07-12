using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Data
{
    public class VendorProfileBuilder : NopEntityBuilder<VendorProfile>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(VendorProfile.VendorId))
                .AsInt32()
                .ForeignKey<Vendor>()
                .OnDelete(System.Data.Rule.Cascade);
        }
    }
}
