using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Data
{
    public class SqlValueParameterBuilder : NopEntityBuilder<SqlParameterValue>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SqlParameterValue.SqlParameterId)).AsInt32().ForeignKey<SqlParameter>()
                .WithColumn(nameof(SqlParameterValue.Value)).AsString().NotNullable();
        }
    }
}
