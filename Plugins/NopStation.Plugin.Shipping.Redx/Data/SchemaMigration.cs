using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Data
{
    [NopMigration("2021/12/12 08:45:55:9687547", "NopStation.Redx base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<RedxArea>();
            Create.TableFor<RedxShipment>();
        }
    }
}