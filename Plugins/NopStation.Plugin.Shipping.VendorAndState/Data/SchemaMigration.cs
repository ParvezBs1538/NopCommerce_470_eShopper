using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Shipping.VendorAndState.Domain;

namespace NopStation.Plugin.Shipping.VendorAndState.Data
{
    [NopMigration("2021/08/13 09:47:55:1687542", "NopStation.VendorAndState base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<VendorShipping>();
            Create.TableFor<VendorStateProvinceShipping>();
        }
    }
}
