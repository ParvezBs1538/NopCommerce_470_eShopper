using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.WidgetPush.Domains;

namespace NopStation.Plugin.Widgets.WidgetPush.Data
{
    [NopMigration("2021/04/27 08:40:55:1687540", "NopStation.WidgetPush base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<WidgetPushItem>();
        }
    }
}