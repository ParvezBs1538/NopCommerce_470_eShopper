using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Data
{
    public class PushNotificationAnnouncementRecordBuilder : NopEntityBuilder<PushNotificationAnnouncement>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PushNotificationAnnouncement.Title))
                .AsString(int.MaxValue)
                .WithColumn(nameof(PushNotificationAnnouncement.Body))
                .AsString(int.MaxValue)
                .Nullable()
                .WithColumn(nameof(PushNotificationAnnouncement.UseDefaultIcon))
                .AsBoolean()
                .WithColumn(nameof(PushNotificationAnnouncement.IconId))
                .AsInt32()
                .WithColumn(nameof(PushNotificationAnnouncement.ImageId))
                .AsInt32()
                .WithColumn(nameof(PushNotificationAnnouncement.Url))
                .AsString(int.MaxValue)
                .Nullable()
                .WithColumn(nameof(PushNotificationAnnouncement.CreatedOnUtc))
                .AsDateTime();
        }
    }
}