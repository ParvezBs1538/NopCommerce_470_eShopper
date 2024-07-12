using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using Nop.Core.Domain.Affiliates;

namespace NopStation.Plugin.Widgets.AffiliateStation.Data
{
    public class OrderCommissionRecordBuilder : NopEntityBuilder<OrderCommission>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(OrderCommission.OrderId)).AsInt32().ForeignKey<Order>().PrimaryKey()
                .WithColumn(nameof(OrderCommission.AffiliateId)).AsInt32().ForeignKey<Affiliate>();
        }
    }
}