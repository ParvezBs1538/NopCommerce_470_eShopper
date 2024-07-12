using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.SMS.TeleSign.Domains;

namespace NopStation.Plugin.SMS.TeleSign.Data
{
    [NopMigration("2021/09/11 08:30:55:1637541", "NopStation.TeleSign base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<SmsTemplate>();
            Create.TableFor<QueuedSms>();
        }
    }
}