using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.MegaMenu.Domains;

namespace NopStation.Plugin.Widgets.MegaMenu.Data;

[NopMigration("2020/09/20 03:10:23:1264924", "NopStation.Plugin.Widgets.MegaMenu base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    protected IMigrationManager _migrationManager;

    public SchemaMigration(IMigrationManager migrationManager)
    {
        _migrationManager = migrationManager;
    }

    public override void Up()
    {
        Create.TableFor<CategoryIcon>();
    }
}
