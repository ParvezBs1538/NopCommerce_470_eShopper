using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.AddressValidator.Google.Domains;

namespace NopStation.Plugin.AddressValidator.Google.Data
{
    [NopMigration("2021/07/23 08:30:55:1687541", "NopStation.GoogleAddressValidator base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<GoogleAddressExtension>();
        }
    }
}