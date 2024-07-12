using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Data
{
    public class OrderDeliverySlotBuilder : NopEntityBuilder<OrderDeliverySlot>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(OrderDeliverySlot.OrderId)).AsInt32().ForeignKey<Order>().PrimaryKey()
                .WithColumn(nameof(OrderDeliverySlot.DeliverySlotId)).AsInt32().ForeignKey<DeliverySlot>()
                .WithColumn(nameof(OrderDeliverySlot.ShippingMethodId)).AsInt32().ForeignKey<ShippingMethod>();
        }
    }
}
