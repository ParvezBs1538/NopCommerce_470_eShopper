using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.CrawlerManager.Domain;

namespace NopStation.Plugin.Widgets.CrawlerManager.Data
{
    [NopMigration("2023/12/12 00:00:00", "NopStation.Plugin.Widgets.CrawlerManager base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<Crawler>();
        }
    }
}