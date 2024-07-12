using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Payout.Stripe.Domain;

namespace NopStation.Plugin.Payout.Stripe.Data
{
    [NopMigration("2023/12/12 17:05:00", "NopStation.VendorStripeConfiguration base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<VendorStripeConfiguration>();
        }
    }
}
