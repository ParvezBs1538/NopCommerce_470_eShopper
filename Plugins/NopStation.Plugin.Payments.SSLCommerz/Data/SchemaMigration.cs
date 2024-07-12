using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Payments.SSLCommerz.Domains;

namespace NopStation.Plugin.Payments.SSLCommerz.Data
{
    [NopMigration("2022/01/20 03:04:55:1687444", "NopStation.SSLCommerz base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<Refund>();
        }
    }
}