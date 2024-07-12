using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Data
{
    [NopMigration("2021/03/03 19:42:00", "NopStation.SqlManager base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<SqlParameter>();
            Create.TableFor<SqlReport>();
            Create.TableFor<SqlParameterValue>();
        }
    }
}
