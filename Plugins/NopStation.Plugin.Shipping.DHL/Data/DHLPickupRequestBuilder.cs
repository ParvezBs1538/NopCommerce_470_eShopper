using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Shipping.DHL.Domain;

namespace NopStation.Plugin.Shipping.DHL.Data
{
    public class DHLPickupRequestBuilder : NopEntityBuilder<DHLPickupRequest>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DHLPickupRequest.MessageReference))
                .AsString().NotNullable()
                .WithColumn(nameof(DHLPickupRequest.ConfirmationNumber))
                .AsString().NotNullable()
                .WithColumn(nameof(DHLPickupRequest.ReadyByTime))
                .AsString().NotNullable()
                .WithColumn(nameof(DHLPickupRequest.NextPickupDate))
                .AsString().NotNullable();
        }        
    }
}
