using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.CRM.Zoho.Domain;

namespace NopStation.Plugin.CRM.Zoho.Data
{
    [NopMigration("2021/09/21 08:40:55:1687541", "NopStation.ZohoCRM base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<UpdatableItem>();
            Create.TableFor<DataMapping>();
        }
    }
}