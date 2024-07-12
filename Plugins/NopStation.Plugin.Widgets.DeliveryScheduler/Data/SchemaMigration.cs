using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Data
{
    [NopMigration("2021/06/18 02:30:55:1687001", "NopStation.DeliveryScheduler base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<DeliverySlot>();
            Create.TableFor<DeliveryCapacity>();
            Create.TableFor<SpecialDeliveryCapacity>();
            Create.TableFor<SpecialDeliveryOffset>();
            Create.TableFor<OrderDeliverySlot>();
            Create.TableFor<DeliverySlotShippingMethodMapping>();
        }
    }
}