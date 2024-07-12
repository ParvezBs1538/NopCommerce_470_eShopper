using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Data.Migrations;

[NopMigration("2021/10/11 12:10:14:2551633", "NopStation.SmartMegaMenu base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<MegaMenu>();
        Create.TableFor<MegaMenuItem>();
    }
}
