using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Data
{
    public class PickupInStoreDeliveryManageBuilder : NopEntityBuilder<PickupInStoreDeliveryManage>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(PickupInStoreDeliveryManage.OrderId))
                .AsInt32()
                .NotNullable()
                .WithColumn(nameof(PickupInStoreDeliveryManage.ReadyForPickupMarkedAtUtc))
                .AsDateTime()
                .Nullable()
                .WithColumn(nameof(PickupInStoreDeliveryManage.CustomerPickedUpAtUtc))
                .AsDateTime()
                .Nullable()
                .WithColumn(nameof(PickupInStoreDeliveryManage.CreatedShipmentId))
                .AsInt32()
                .Nullable();
        }
    }
}
