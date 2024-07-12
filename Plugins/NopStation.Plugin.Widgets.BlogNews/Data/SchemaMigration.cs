using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widget.BlogNews.Domains;

namespace NopStation.Plugin.Widget.BlogNews.Data;

[NopMigration("2021/12/30 09:55:55:1687542", "NopStation.Plugin.Widget.BlogNews base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    protected IMigrationManager _migrationManager;
    public SchemaMigration(IMigrationManager migrationManager)
    {
        _migrationManager = migrationManager;
    }
    public override void Up()
    {
        Create.TableFor<BlogNewsPicture>();
    }
}
