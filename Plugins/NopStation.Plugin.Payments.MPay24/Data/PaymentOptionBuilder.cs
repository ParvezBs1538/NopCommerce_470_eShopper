using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payments.MPay24.Domains;

namespace NopStation.Plugin.Payments.MPay24.Mapping.Data
{
    public class PaymentOptionBuilder : NopEntityBuilder<PaymentOption>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PaymentOption.PaymentType)).AsString(100).NotNullable()
                .WithColumn(nameof(PaymentOption.DisplayName)).AsString(100).NotNullable()
                .WithColumn(nameof(PaymentOption.Brand)).AsString(100).Nullable()
                .WithColumn(nameof(PaymentOption.ShortName)).AsString(100).NotNullable()
                .WithColumn(nameof(PaymentOption.Description)).AsString(1000).Nullable();
        }

        #endregion
    }
}