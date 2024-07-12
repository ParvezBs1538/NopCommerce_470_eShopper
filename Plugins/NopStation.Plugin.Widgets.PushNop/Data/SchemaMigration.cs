using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Data
{
    [NopMigration("2021/06/29 09:47:55:1687542", "NopStation.PushNop base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<SmartGroup>();
            Create.TableFor<SmartGroupCondition>();
            Create.TableFor<SmartGroupNotification>();
        }
    }
}
