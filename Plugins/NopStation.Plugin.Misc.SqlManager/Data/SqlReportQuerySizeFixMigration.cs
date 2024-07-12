using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Data
{
    [NopMigration("2023/05/30 17:58:00", "NopStation.SqlManager SqlReport Query column size fix migration", MigrationProcessType.Update)]
    public class SqlReportQuerySizeFixMigration : Migration
    {
        public override void Down()
        {
            // no need to reduce size
        }

        public override void Up()
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(SqlReport)))
                .AlterColumn(nameof(SqlReport.Query))
                .AsString(int.MaxValue)
                .NotNullable();
        }
    }
}
