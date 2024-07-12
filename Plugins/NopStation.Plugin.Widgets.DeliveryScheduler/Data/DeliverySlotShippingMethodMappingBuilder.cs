using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Data
{
    public class DeliverySlotShippingMethodMappingBuilder : NopEntityBuilder<DeliverySlotShippingMethodMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DeliverySlotShippingMethodMapping.DeliverySlotId)).AsInt32().ForeignKey<DeliverySlot>().PrimaryKey()
                .WithColumn(nameof(DeliverySlotShippingMethodMapping.ShippingMethodId)).AsInt32().ForeignKey<ShippingMethod>().PrimaryKey();
        }
    }
}