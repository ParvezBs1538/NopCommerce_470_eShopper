using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Shipping;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data
{
    public class ShipmentNoteBuilder : NopEntityBuilder<ShipmentNote>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ShipmentNote.CourierShipmentId)).AsInt32().ForeignKey<CourierShipment>().OnDelete(Rule.Cascade)
                .WithColumn(nameof(ShipmentNote.NopShipmentId)).AsInt32().ForeignKey<Shipment>().OnDelete(Rule.Cascade);
        }
    }
}
