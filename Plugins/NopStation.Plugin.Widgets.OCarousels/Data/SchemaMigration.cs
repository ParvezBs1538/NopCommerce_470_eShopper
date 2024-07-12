using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Data
{
    [NopMigration("2020/07/08 08:41:55:1687543", "NopStation.OCarousels base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<OCarousel>();
            Create.TableFor<OCarouselItem>();
        }
    }
}
