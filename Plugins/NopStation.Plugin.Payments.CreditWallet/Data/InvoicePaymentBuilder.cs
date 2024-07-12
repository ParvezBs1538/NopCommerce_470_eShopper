using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Mapping.Data
{
    public class InvoicePaymentBuilder : NopEntityBuilder<InvoicePayment>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(InvoicePayment.WalletCustomerId)).AsInt32().ForeignKey<Wallet>(primaryColumnName: "WalletCustomerId");
        }

        #endregion
    }
}