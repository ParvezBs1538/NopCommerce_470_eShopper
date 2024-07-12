using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Shipping.DHL.Domain;

namespace NopStation.Plugin.Shipping.DHL
{
    [NopMigration("2022/01/02 08:50:30:1278351", "NopStation.DHL base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<DHLShipment>();
            Create.TableFor<DHLPickupRequest>();
            Create.TableFor<DHLService>();
        }
    }
}