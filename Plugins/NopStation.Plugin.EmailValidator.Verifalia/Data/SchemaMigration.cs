using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.EmailValidator.Verifalia.Domains;

namespace NopStation.Plugin.EmailValidator.Verifalia.Data
{
    [NopMigration("2022/09/17 08:30:55:1687520", "NopStation.Verifalia base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<VerifaliaEmail>();
        }
    }
}