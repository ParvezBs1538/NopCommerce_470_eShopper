using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Mapping.Data
{
    public class ActivityHistoryBuilder : NopEntityBuilder<ActivityHistory>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ActivityHistory.WalletCustomerId)).AsInt32().ForeignKey<Wallet>(primaryColumnName: "WalletCustomerId");
        }

        #endregion
    }
}