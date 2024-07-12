using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Payments.Quickstream.Domains;

namespace NopStation.Plugin.Payments.Quickstream.Data
{
    [NopMigration("2020/05/05 08:40:55:1686441", "NopStation.QuickstreamPayment base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            Create.TableFor<AcceptedCard>();
        }
    }
}
