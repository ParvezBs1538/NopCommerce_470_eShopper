using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Mapping.Data
{
    public class WalletBuilder : NopEntityBuilder<Wallet>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Wallet.WalletCustomerId)).AsInt32().ForeignKey<Customer>().PrimaryKey()
                .WithColumn(nameof(Wallet.CurrencyId)).AsInt32().ForeignKey<Currency>();
        }

        #endregion
    }
}