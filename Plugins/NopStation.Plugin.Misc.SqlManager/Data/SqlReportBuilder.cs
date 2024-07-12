using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Data
{
    public class SqlReportBuilder : NopEntityBuilder<SqlReport>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SqlReport.Name)).AsString().NotNullable()
                .WithColumn(nameof(SqlReport.Query)).AsString(int.MaxValue).NotNullable();
        }
    }
}