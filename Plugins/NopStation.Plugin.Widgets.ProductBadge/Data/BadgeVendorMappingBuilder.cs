using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Data;

public class BadgeVendorMappingBuilder : NopEntityBuilder<BadgeVendorMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(BadgeVendorMapping.BadgeId)).AsInt32().ForeignKey<Badge>()
             .WithColumn(nameof(BadgeVendorMapping.VendorId)).AsInt32().ForeignKey<Vendor>();
    }
}