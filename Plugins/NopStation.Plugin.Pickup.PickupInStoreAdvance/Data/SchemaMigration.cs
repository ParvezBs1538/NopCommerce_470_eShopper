using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Data
{
    [NopMigration("2020/02/03 09:30:17:6455458", "NopStation.PickupInStoreAdvance base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<StorePickupPoint>();
            Create.TableFor<PickupInStoreDeliveryManage>();
        }
    }
}