using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Data
{
    public class HelpdeskDepartmentRecordBuilder : NopEntityBuilder<HelpdeskDepartment>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(HelpdeskDepartment.Name)).AsString().NotNullable()
                .WithColumn(nameof(HelpdeskDepartment.DisplayOrder)).AsInt32().NotNullable();
        }
    }
}