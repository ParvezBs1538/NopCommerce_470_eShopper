using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Data
{
    [NopMigration("2022/06/08 08:30:55:1687123", "NopStation.Plugin.Misc.AjaxFilter base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            Create.TableFor<AjaxFilterParentCategory>();
            Create.TableFor<AjaxFilterSpecificationAttribute>();
        }
    }
}
