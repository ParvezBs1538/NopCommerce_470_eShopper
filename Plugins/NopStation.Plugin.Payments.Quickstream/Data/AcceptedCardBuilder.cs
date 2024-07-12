using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payments.Quickstream.Domains;

namespace NopStation.Plugin.Payments.Quickstream.Data
{
    public class AcceptedCardBuilder : NopEntityBuilder<AcceptedCard>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AcceptedCard.CardScheme)).AsString().NotNullable()
                .WithColumn(nameof(AcceptedCard.CardType)).AsString().NotNullable()
                .WithColumn(nameof(AcceptedCard.PictureId)).AsInt32().Nullable()
                .WithColumn(nameof(AcceptedCard.Surcharge)).AsDouble().Nullable()
                .WithColumn(nameof(AcceptedCard.IsEnable)).AsBoolean().Nullable();
        }
    }
}
