using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.EmailValidator.Abstract.Domains;

namespace NopStation.Plugin.EmailValidator.Abstract.Data
{
    [NopMigration("2022/09/19 08:30:55:1687500", "NopStation.Abstract base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<AbstractEmail>();
        }
    }
}