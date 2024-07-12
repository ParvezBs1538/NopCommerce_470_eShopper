using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.Announcement.Domains;

namespace NopStation.Plugin.Widgets.Announcement.Data;

public class AnnouncementItemRecordBuilder : NopEntityBuilder<AnnouncementItem>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(AnnouncementItem.Name)).AsString().NotNullable()
            .WithColumn(nameof(AnnouncementItem.Title)).AsString(3000).NotNullable()
            .WithColumn(nameof(AnnouncementItem.DisplayOrder)).AsInt32();
    }
}