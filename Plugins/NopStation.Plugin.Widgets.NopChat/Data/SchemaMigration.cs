using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.NopChat.Domains;

namespace NopStation.Plugin.Widgets.NopChat.Data
{
    [NopMigration("2021/10/16 08:40:55:1687547", "NopStation.Plugin.Widgets.NopChat base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            Create.TableFor<NopChatMessage>();
        }
    }
}
