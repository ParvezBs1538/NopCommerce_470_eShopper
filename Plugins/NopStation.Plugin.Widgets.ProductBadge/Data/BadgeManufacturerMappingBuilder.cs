using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Data;

public class BadgeManufacturerMappingBuilder : NopEntityBuilder<BadgeManufacturerMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(BadgeManufacturerMapping.BadgeId)).AsInt32().ForeignKey<Badge>()
             .WithColumn(nameof(BadgeManufacturerMapping.ManufacturerId)).AsInt32().ForeignKey<Manufacturer>();
    }
}