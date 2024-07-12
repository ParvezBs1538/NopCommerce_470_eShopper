using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Shipping;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data
{
    public class CourierShipmentBuilder : NopEntityBuilder<CourierShipment>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CourierShipment.ShipperId)).AsInt32().ForeignKey<Shipper>(onDelete: Rule.None)
                .WithColumn(nameof(CourierShipment.ShipmentPickupPointId)).AsInt32().ForeignKey<ShipmentPickupPoint>(onDelete: Rule.None)
                .WithColumn(nameof(CourierShipment.ShipmentId)).AsInt32().ForeignKey<Shipment>(onDelete: Rule.None);
        }
    }
}
