using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.SMS.Messente.Domains;

namespace NopStation.Plugin.SMS.Messente.Data
{
    [NopMigration("2021/07/30 08:30:57:1687541", "NopStation.Messente base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<SmsTemplate>();
            Create.TableFor<QueuedSms>();
        }
    }
}