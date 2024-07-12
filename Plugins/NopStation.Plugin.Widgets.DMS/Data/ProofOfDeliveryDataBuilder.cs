using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Shipping;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Data
{
    public class ProofOfDeliveryDataBuilder : NopEntityBuilder<ProofOfDeliveryData>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ProofOfDeliveryData.NopShipmentId)).AsInt32().ForeignKey<Shipment>()
                .WithColumn(nameof(ProofOfDeliveryData.CourierShipmentId)).AsInt32().ForeignKey<CourierShipment>()
                .WithColumn(nameof(ProofOfDeliveryData.VerifiedByShipperId)).AsInt32().ForeignKey<Shipper>();
        }
    }
}
