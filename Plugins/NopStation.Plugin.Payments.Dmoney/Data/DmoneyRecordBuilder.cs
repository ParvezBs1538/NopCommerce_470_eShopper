using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Payments.Dmoney.Domains;

namespace NopStation.Plugin.Payments.Dmoney.Data
{
    public class DmoneyRecordBuilder : NopEntityBuilder<DmoneyTransaction>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DmoneyTransaction.StatusCode)).AsInt32()
                .WithColumn(nameof(DmoneyTransaction.ErrorMessage)).AsString(1000).Nullable()
                .WithColumn(nameof(DmoneyTransaction.MerchantWalletNumber)).AsString(500).Nullable()
                .WithColumn(nameof(DmoneyTransaction.CustomerWalletNumber)).AsString(500).Nullable()
                .WithColumn(nameof(DmoneyTransaction.Amount)).AsDecimal(18, 2)
                .WithColumn(nameof(DmoneyTransaction.TransactionType)).AsString(500).Nullable()
                .WithColumn(nameof(DmoneyTransaction.OrderId)).AsInt32().ForeignKey<Order>()
                .WithColumn(nameof(DmoneyTransaction.TransactionTime)).AsString(500).Nullable()
                .WithColumn(nameof(DmoneyTransaction.PaymentStatus)).AsString(500).Nullable()
                .WithColumn(nameof(DmoneyTransaction.TransactionReferenceId)).AsString(500).Nullable()
                .WithColumn(nameof(DmoneyTransaction.TransactionTrackingNumber)).AsString(500).Nullable()
                .WithColumn(nameof(DmoneyTransaction.StatusMessage)).AsString(500).Nullable()
                .WithColumn(nameof(DmoneyTransaction.ErrorCode)).AsString(500).Nullable()
                .WithColumn(nameof(DmoneyTransaction.TransactionStatus)).AsInt32()
                .WithColumn(nameof(DmoneyTransaction.CreatedOnUtc)).AsDateTime()
                .WithColumn(nameof(DmoneyTransaction.LastUpdatedOnUtc)).AsDateTime();
        }
    }
}