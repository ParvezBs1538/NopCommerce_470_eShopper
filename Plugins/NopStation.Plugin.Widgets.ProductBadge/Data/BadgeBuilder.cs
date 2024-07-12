using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Data;

public class BadgeBuilder : NopEntityBuilder<Badge>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Badge.Name)).AsString(400).NotNullable();
    }
}