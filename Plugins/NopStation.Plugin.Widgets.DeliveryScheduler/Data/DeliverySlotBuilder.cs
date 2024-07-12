using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Data
{
    public class DeliverySlotBuilder : NopEntityBuilder<DeliverySlot>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DeliverySlot.TimeSlot)).AsString();
        }
    }
}