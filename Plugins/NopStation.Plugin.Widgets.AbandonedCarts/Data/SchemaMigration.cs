using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Data
{
    [NopMigration("2023/08/10 00:00:00", "NopStation.Plugin.Widgets.AbandonedCarts base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<AbandonedCart>();
            Create.TableFor<CustomerAbandonmentInfo>();
        }
    }
}