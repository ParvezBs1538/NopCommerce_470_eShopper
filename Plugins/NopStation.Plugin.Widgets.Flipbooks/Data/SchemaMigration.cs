using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.Flipbooks.Domains;

namespace NopStation.Plugin.Widgets.Flipbooks.Data
{
    [NopMigration("2021/05/12 08:40:55:1687542", "NopStation.Flipbooks schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<Flipbook>();
            Create.TableFor<FlipbookContent>();
            Create.TableFor<FlipbookContentProduct>();
        }
    }
}
