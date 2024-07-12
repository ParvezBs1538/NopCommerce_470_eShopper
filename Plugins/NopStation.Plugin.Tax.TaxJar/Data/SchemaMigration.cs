using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Tax.TaxJar.Domains;

namespace NopStation.Plugin.Tax.TaxJar.Data
{
    [NopMigration("2021/08/01 19:42:00", "NopStation.TaxJar base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<TaxJarCategory>();
            Create.TableFor<TaxjarTransactionLog>();
        }
    }
}
