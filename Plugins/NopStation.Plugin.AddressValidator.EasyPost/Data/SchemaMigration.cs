using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.AddressValidator.EasyPost.Domains;

namespace NopStation.Plugin.AddressValidator.EasyPost.Data
{
    [NopMigration("2021/07/26 08:30:55:1687541", "NopStation.EasyPostAddressValidator base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<EasyPostAddressExtension>();
        }
    }
}