using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using NopStation.Plugin.Shipping.Redx.Domains;
using Nop.Core.Domain.Shipping;
using System.Data;

namespace NopStation.Plugin.Shipping.Redx.Data
{
    public class RedxShipmentBuilder : NopEntityBuilder<RedxShipment>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(RedxShipment.OrderId)).AsInt32().ForeignKey<Order>()
                .WithColumn(nameof(RedxShipment.RedxAreaId)).AsInt32().ForeignKey<RedxArea>(primaryColumnName: "RedxAreaId")
                .WithColumn(nameof(RedxShipment.ShipmentId)).AsInt32().ForeignKey<Shipment>(onDelete: Rule.None);
        }
    }
}