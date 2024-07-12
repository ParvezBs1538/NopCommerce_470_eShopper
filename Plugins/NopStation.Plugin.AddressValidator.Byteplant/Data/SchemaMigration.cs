using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.AddressValidator.Byteplant.Domains;

namespace NopStation.Plugin.AddressValidator.Byteplant.Data
{
    [NopMigration("2021/09/23 08:30:55:1687541", "NopStation.ByteplantAddressValidator base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<ByteplantAddressExtension>();
        }
    }
}