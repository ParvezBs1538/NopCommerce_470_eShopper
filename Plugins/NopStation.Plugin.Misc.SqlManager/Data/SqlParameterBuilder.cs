using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Data
{
    public class SqlParameterBuilder : NopEntityBuilder<SqlParameter>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SqlParameter.Name)).AsString().NotNullable()
                .WithColumn(nameof(SqlParameter.SystemName)).AsString().NotNullable();
        }
    }
}
