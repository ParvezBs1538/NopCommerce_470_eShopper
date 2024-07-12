using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Data
{
    [NopMigration("2020/07/08 08:30:55:1687541", "NopStation.WebApiNotification base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<WebApiNotificationCampaign>();
            Create.TableFor<WebApiNotificationTemplate>();
            Create.TableFor<WebApiQueuedNotification>();
        }
    }
}
