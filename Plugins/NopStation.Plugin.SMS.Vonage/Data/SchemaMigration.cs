using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.SMS.Vonage.Domains;

namespace NopStation.Plugin.SMS.Vonage.Data
{
    [NopMigration("2021/01/11 08:30:55:1630541", "NopStation.VonageSms base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<SmsTemplate>();
            Create.TableFor<QueuedSms>();
        }
    }
}