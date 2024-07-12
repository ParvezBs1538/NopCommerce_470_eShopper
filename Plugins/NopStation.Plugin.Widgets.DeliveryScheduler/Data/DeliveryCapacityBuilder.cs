using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Data
{
    public class DeliveryCapacityBuilder : NopEntityBuilder<DeliveryCapacity>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DeliveryCapacity.DeliverySlotId)).AsInt32().ForeignKey<DeliverySlot>().PrimaryKey()
                .WithColumn(nameof(DeliveryCapacity.ShippingMethodId)).AsInt32().ForeignKey<ShippingMethod>().PrimaryKey();
        }
    }
}