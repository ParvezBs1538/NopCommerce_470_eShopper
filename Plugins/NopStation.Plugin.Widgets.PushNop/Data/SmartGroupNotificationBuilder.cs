using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Data
{
    public class SmartGroupNotificationBuilder : NopEntityBuilder<SmartGroupNotification>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
               .WithColumn(nameof(SmartGroupNotification.SmartGroupId)).AsInt32().ForeignKey<SmartGroup>().Nullable();
        }
    }
}