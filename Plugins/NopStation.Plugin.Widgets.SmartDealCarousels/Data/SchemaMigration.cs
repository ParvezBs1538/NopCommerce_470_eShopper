using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Data;

[NopMigration("2022/11/08 08:41:55:1687533", "NopStation.SmartDealCarousels base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<SmartDealCarousel>();
        Create.TableFor<SmartDealCarouselProductMapping>();
    }
}
