using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Data;

[NopMigration("2022/08/23 04:43:55:1687099", "NopStation.ProductBadge base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<Badge>();
        Create.TableFor<BadgeCategoryMapping>();
        Create.TableFor<BadgeManufacturerMapping>();
        Create.TableFor<BadgeProductMapping>();
        Create.TableFor<BadgeVendorMapping>();
    }
}