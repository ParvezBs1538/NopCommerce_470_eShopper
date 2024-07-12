using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Data;

public class BadgeCategoryMappingBuilder : NopEntityBuilder<BadgeCategoryMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(BadgeCategoryMapping.BadgeId)).AsInt32().ForeignKey<Badge>()
             .WithColumn(nameof(BadgeCategoryMapping.CategoryId)).AsInt32().ForeignKey<Category>();
    }
}