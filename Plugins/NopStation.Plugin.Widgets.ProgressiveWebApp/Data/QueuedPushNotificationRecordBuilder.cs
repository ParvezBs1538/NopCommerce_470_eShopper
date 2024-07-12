using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Data
{
    public class QueuedPushNotificationRecordBuilder : NopEntityBuilder<QueuedPushNotification>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(QueuedPushNotification.CustomerId))
                .AsInt32()
                .Nullable()
                .WithColumn(nameof(QueuedPushNotification.StoreId))
                .AsInt32()
                .WithColumn(nameof(QueuedPushNotification.Title))
                .AsString(int.MaxValue)
                .WithColumn(nameof(QueuedPushNotification.Body))
                .AsString(int.MaxValue)
                .Nullable()
                .WithColumn(nameof(QueuedPushNotification.IconUrl))
                .AsString(int.MaxValue)
                .Nullable()
                .WithColumn(nameof(QueuedPushNotification.ImageUrl))
                .AsString(int.MaxValue)
                .Nullable()
                .WithColumn(nameof(QueuedPushNotification.Url))
                .AsString(int.MaxValue)
                .Nullable()
                .WithColumn(nameof(QueuedPushNotification.Rtl))
                .AsBoolean()
                .WithColumn(nameof(QueuedPushNotification.CreatedOnUtc))
                .AsDateTime()
                .WithColumn(nameof(QueuedPushNotification.SentOnUtc))
                .AsDateTime()
                .Nullable();
        }
    }
}