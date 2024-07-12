using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payments.Affirm.Domain;

namespace NopStation.Plugin.Payments.Affirm.Data
{
    public class AffirmPaymentTransactionBuilder : NopEntityBuilder<AffirmPaymentTransaction>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            //table
            //    .WithColumn(nameof(AffirmPaymentTransaction.OrderGuid)).AsInt32().ForeignKey<Order>();
        }
    }
}
