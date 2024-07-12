using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Shipping;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data
{
    public class DeliverFailedRecordsBuilder : NopEntityBuilder<DeliverFailedRecord>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DeliverFailedRecord.ShipmentId)).AsInt32().ForeignKey<Shipment>()
                .WithColumn(nameof(DeliverFailedRecord.CourierShipmentId)).AsInt32().ForeignKey<CourierShipment>()
                .WithColumn(nameof(DeliverFailedRecord.ShipperId)).AsInt32().ForeignKey<Shipper>();
        }
    }
}
