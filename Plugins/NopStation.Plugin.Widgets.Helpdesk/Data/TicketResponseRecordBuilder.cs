using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Data
{
    public class TicketResponseRecordBuilder : NopEntityBuilder<TicketResponse>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(TicketResponse.TicketId)).AsInt32().ForeignKey<Ticket>();
        }
    }
}
