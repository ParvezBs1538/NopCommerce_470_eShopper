using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Data
{
    [NopMigration("2021/03/01 08:30:55:1687541", "NopStation.ProgressiveWebApp base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<WebAppDevice>();
            Create.TableFor<PushNotificationAnnouncement>();
            Create.TableFor<PushNotificationTemplate>();
            Create.TableFor<QueuedPushNotification>();
            Create.TableFor<AbandonedCartTracking>();
        }
    }
}