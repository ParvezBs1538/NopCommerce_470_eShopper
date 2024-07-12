using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories
{
    public interface ITicketModelFactory
    {
        Task<TicketSearchModel> PrepareTicketSearchModel(TicketSearchModel searchModel);

        Task<TicketListModel> PrepareTicketListModelAsync(TicketSearchModel searchModel);

        Task<TicketModel> PrepareTicketModelAsync(TicketModel model, Ticket ticket, bool excludeProperties = false);

        Task<ResponseListModel> PrepareResponseListModelAsync(ResponseSearchModel searchModel, Ticket ticket);
    }
}
