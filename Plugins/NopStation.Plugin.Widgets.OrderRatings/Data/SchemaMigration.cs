using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.OrderRatings.Domain;

namespace NopStation.Plugin.Widgets.OrderRatings.Data
{
    [NopMigration("2021/06/14 11:50:00:6455422", "NopStation.OrderRatings base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<OrderRating>();
        }
    }
}
