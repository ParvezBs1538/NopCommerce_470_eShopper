﻿using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Data;

public class BadgeProductMappingBuilder : NopEntityBuilder<BadgeProductMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
             .WithColumn(nameof(BadgeProductMapping.BadgeId)).AsInt32().ForeignKey<Badge>()
             .WithColumn(nameof(BadgeProductMapping.ProductId)).AsInt32().ForeignKey<Product>();
    }
}