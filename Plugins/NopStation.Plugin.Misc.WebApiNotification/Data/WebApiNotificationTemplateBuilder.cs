using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Data
{
    public class WebApiNotificationTemplateBuilder : NopEntityBuilder<WebApiNotificationTemplate>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WebApiNotificationTemplate.ImageId)).AsInt32()
                .WithColumn(nameof(WebApiNotificationTemplate.Active)).AsBoolean()
                .WithColumn(nameof(WebApiNotificationTemplate.LimitedToStores)).AsBoolean()
                .WithColumn(nameof(WebApiNotificationTemplate.SendImmediately)).AsBoolean()
                .WithColumn(nameof(WebApiNotificationTemplate.DelayBeforeSend)).AsInt32().Nullable()
                .WithColumn(nameof(WebApiNotificationTemplate.DelayPeriodId)).AsInt32()
                .WithColumn(nameof(WebApiNotificationTemplate.ActionTypeId)).AsInt32()
                .WithColumn(nameof(WebApiNotificationTemplate.ActionValue)).AsString().Nullable();
        }
    }
}