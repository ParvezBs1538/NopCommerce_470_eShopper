using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data
{
    [NopMigration("2023/10/05 09:40:55:1687121", "NopStation.DMS base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<Shipper>();
            Create.TableFor<OTPRecord>();
            Create.TableFor<ShipmentPickupPoint>();
            Create.TableFor<CourierShipment>();
            Create.TableFor<DeliverFailedRecord>();
            Create.TableFor<ProofOfDeliveryData>();
            Create.TableFor<ShipmentNote>();
            Create.TableFor<ShipperDevice>();
        }
    }
}
