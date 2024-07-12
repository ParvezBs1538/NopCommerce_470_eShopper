using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payments.SSLCommerz.Domains;

namespace NopStation.Plugin.Payments.SSLCommerz.Data
{
    public class RefundBuilder : NopEntityBuilder<Refund>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Refund.OrderId)).AsInt32().ForeignKey<Order>();
        }
    }
}
