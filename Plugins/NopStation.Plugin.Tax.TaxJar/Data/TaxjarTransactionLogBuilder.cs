using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Tax.TaxJar.Domains;

namespace NopStation.Plugin.Tax.TaxJar.Data
{
    public class TaxjarTransactionLogBuilder : NopEntityBuilder<TaxjarTransactionLog>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(TaxjarTransactionLog.TransactionId)).AsString();
        }
    }
}
