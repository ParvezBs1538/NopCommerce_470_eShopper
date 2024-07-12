using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Data
{
    [NopMigration("2021/12/13 17:05:17:6455422", "NopStation.CustomerCreditsPayment base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<Wallet>();
            Create.TableFor<InvoicePayment>();
            Create.TableFor<ActivityHistory>();
        }
    }
}
