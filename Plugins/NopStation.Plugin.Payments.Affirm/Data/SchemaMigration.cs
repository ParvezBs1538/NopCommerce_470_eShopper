using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Payments.Affirm.Domain;

namespace NopStation.Plugin.Payments.Affirm.Data
{
    [NopMigration("2022/03/20 03:04:55:1687444", "NopStation.Plugin.Payments.Affirm base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<AffirmPaymentTransaction>();
        }
    }
}