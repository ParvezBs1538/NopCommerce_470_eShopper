using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.SMS.Twilio.Domains;

namespace NopStation.Plugin.SMS.Twilio.Data
{
    [NopMigration("2021/01/11 08:30:55:1687541", "NopStation.Twilio base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<SmsTemplate>();
            Create.TableFor<QueuedSms>();
        }
    }
}