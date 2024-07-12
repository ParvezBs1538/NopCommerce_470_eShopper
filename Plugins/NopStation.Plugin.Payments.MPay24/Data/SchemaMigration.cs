using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Payments.MPay24.Domains;

namespace NopStation.Plugin.Payments.MPay24.Data
{
    [NopMigration("2021/10/31 07:25:59:1609121", "NopStation.MPay24 schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<PaymentOption>();
        }
    }
}
