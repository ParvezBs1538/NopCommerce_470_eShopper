using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.SMS.BulkSms.Domains;

namespace NopStation.Plugin.SMS.BulkSms.Data
{
    [NopMigration("2021/07/30 08:30:56:1687541", "NopStation.BulkSms base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<SmsTemplate>();
            Create.TableFor<QueuedSms>();
        }
    }
}