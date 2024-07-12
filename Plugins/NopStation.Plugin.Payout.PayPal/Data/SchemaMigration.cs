using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Payout.PayPal.Domain;

namespace NopStation.Plugin.Payout.PayPal.Data
{
    [NopMigration("2023/12/11 20:30:00", "NopStation.VendorPayPalConfiguration base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<VendorPayPalConfiguration>();
        }
    }
}
