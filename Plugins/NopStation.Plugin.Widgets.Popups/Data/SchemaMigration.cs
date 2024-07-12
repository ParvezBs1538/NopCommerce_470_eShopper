using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.Popups.Domains;

namespace NopStation.Plugin.Widgets.Popups.Data;

[NopMigration("2021/07/06 12:24:16:2552018", "NopStation.NewsletterPopups base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<Popup>();
    }
}
