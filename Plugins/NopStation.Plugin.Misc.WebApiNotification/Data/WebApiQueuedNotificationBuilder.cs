using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Data
{
    public class WebApiQueuedNotificationBuilder : NopEntityBuilder<WebApiQueuedNotification>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WebApiQueuedNotification.CustomerId)).AsInt32()
                .WithColumn(nameof(WebApiQueuedNotification.StoreId)).AsInt32()
                .WithColumn(nameof(WebApiQueuedNotification.ImageUrl)).AsString().Nullable()
                .WithColumn(nameof(WebApiQueuedNotification.SentTries)).AsInt32()
                .WithColumn(nameof(WebApiQueuedNotification.AppDeviceId)).AsInt32()
                .WithColumn(nameof(WebApiQueuedNotification.DeviceTypeId)).AsInt32()
                .WithColumn(nameof(WebApiQueuedNotification.ActionTypeId)).AsInt32()
                .WithColumn(nameof(WebApiQueuedNotification.ActionValue)).AsString().Nullable()
                .WithColumn(nameof(WebApiQueuedNotification.SubscriptionId)).AsString().Nullable()
                .WithColumn(nameof(WebApiQueuedNotification.CreatedOnUtc)).AsDateTime()
                .WithColumn(nameof(WebApiQueuedNotification.SentOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(WebApiQueuedNotification.DontSendBeforeDateUtc)).AsDateTime().Nullable();
        }
    }
}