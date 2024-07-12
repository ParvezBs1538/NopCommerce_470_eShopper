using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.WidgetPush.Domains;

namespace NopStation.Plugin.Widgets.WidgetPush.Data
{
    public class WidgetPushItemRecordBuilder : NopEntityBuilder<WidgetPushItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WidgetPushItem.Content)).AsString(int.MaxValue).NotNullable()
                .WithColumn(nameof(WidgetPushItem.Name)).AsString().NotNullable()
                .WithColumn(nameof(WidgetPushItem.DisplayOrder)).AsInt64()
                .WithColumn(nameof(WidgetPushItem.WidgetZone)).AsString().NotNullable();
        }
    }
}