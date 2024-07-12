using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.CategoryBanners.Domains;

namespace NopStation.Plugin.Widgets.CategoryBanners.Data
{
    [NopMigration("2021/03/04 08:15:54:1687541", "NopStation.CategoryBanners base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            Create.TableFor<CategoryBanner>();

        }
    }
}