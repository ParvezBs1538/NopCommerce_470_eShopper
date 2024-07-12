using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Data
{
    public class PushNotificationTemplateRecordBuilder : NopEntityBuilder<PushNotificationTemplate>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PushNotificationTemplate.Name))
                .AsString(int.MaxValue)
                .WithColumn(nameof(PushNotificationTemplate.Title))
                .AsString(int.MaxValue)
                .WithColumn(nameof(PushNotificationTemplate.Body))
                .AsString(int.MaxValue)
                .Nullable()
                .WithColumn(nameof(PushNotificationTemplate.UseDefaultIcon))
                .AsBoolean()
                .WithColumn(nameof(PushNotificationTemplate.IconId))
                .AsInt32()
                .WithColumn(nameof(PushNotificationTemplate.ImageId))
                .AsInt32()
                .WithColumn(nameof(PushNotificationTemplate.Url))
                .AsString(int.MaxValue)
                .Nullable()
                .WithColumn(nameof(PushNotificationTemplate.Active))
                .AsBoolean()
                .WithColumn(nameof(PushNotificationTemplate.LimitedToStores))
                .AsBoolean();
        }
    }
}