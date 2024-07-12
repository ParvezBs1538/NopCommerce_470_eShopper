using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.Helpdesk.Models
{
    public class TicketListModel
    {
        public TicketListModel()
        {
            Tickets = new List<TicketOverviewModel>();
        }

        public bool EnableTicketCategory { get; set; }

        public bool EnableTicketDepartment { get; set; }

        public IList<TicketOverviewModel> Tickets { get; set; }
    }
}
