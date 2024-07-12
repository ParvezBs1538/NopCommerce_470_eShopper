using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using Nop.Data.Extensions;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Data
{
    public class SpecialDeliveryCapacityBuilder : NopEntityBuilder<SpecialDeliveryCapacity>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SpecialDeliveryCapacity.DeliverySlotId)).AsInt32().ForeignKey<DeliverySlot>()
                .WithColumn(nameof(SpecialDeliveryCapacity.ShippingMethodId)).AsInt32().ForeignKey<ShippingMethod>();
        }
    }
}
