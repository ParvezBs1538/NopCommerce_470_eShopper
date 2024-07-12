using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Data
{
    public class WebApiNotificationCampaignBuilder : NopEntityBuilder<WebApiNotificationCampaign>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WebApiNotificationCampaign.ImageId)).AsInt32()
                .WithColumn(nameof(WebApiNotificationCampaign.SendingWillStartOnUtc)).AsDateTime()
                .WithColumn(nameof(WebApiNotificationCampaign.AddedToQueueOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(WebApiNotificationCampaign.LimitedToStoreId)).AsInt32()
                .WithColumn(nameof(WebApiNotificationCampaign.Deleted)).AsBoolean()
                .WithColumn(nameof(WebApiNotificationCampaign.ActionTypeId)).AsInt32()
                .WithColumn(nameof(WebApiNotificationCampaign.ActionValue)).AsString().Nullable()
                .WithColumn(nameof(WebApiNotificationCampaign.CreatedOnUtc)).AsDateTime();
        }
    }
}
