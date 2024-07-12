using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.Helpdesk.Domains;
using NopStation.Plugin.Widgets.Helpdesk.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Factories
{
    public interface ITicketModelFactory
    {
        Task<TicketListModel> PrepareTicketListModelAsync(IPagedList<Ticket> tickets);

        Task<TicketOverviewModel> PrepareTicketOverviewModelAsync(Ticket ticket);

        Task<TicketModel> PrepareAddNewTicketModelAsync(TicketModel model);

        Task<TicketDetailsModel> PrepareTicketDetailsModelAsync(Ticket ticket);
    }
}
