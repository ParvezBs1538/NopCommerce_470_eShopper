using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.SMS.BulkSms.Domains;

namespace NopStation.Plugin.SMS.BulkSms.Data
{
    public class QueuedSmsRecordBuilder : NopEntityBuilder<QueuedSms>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(QueuedSms.CustomerId))
                .AsInt32()
                .Nullable()
                .WithColumn(nameof(QueuedSms.StoreId))
                .AsInt32()
                .WithColumn(nameof(QueuedSms.Body))
                .AsString(int.MaxValue)
                .NotNullable()
                .WithColumn(nameof(QueuedSms.CreatedOnUtc))
                .AsDateTime()
                .WithColumn(nameof(QueuedSms.SentOnUtc))
                .AsDateTime()
                .Nullable();
        }
    }
}