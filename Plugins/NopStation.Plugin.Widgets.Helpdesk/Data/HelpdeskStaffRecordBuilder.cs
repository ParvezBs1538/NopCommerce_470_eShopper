using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Data
{
    public class HelpdeskStaffRecordBuilder : NopEntityBuilder<HelpdeskStaff>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(HelpdeskStaff.Name)).AsString().NotNullable()
                .WithColumn(nameof(HelpdeskStaff.DisplayOrder)).AsInt32().NotNullable();
        }
    }
}