using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.SMS.SmsTo.Domains;

namespace NopStation.Plugin.SMS.SmsTo.Data
{
    [NopMigration("2021/01/11 08:30:55:1637541", "NopStation.SmsToSms base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<SmsTemplate>();
            Create.TableFor<QueuedSms>();
        }
    }
}