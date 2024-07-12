using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.SMS.MessageBird.Domains;

namespace NopStation.Plugin.SMS.MessageBird.Data
{
    [NopMigration("2021/07/30 08:30:55:1777541", "NopStation.MessageBird base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<SmsTemplate>();
            Create.TableFor<QueuedSms>();
        }
    }
}