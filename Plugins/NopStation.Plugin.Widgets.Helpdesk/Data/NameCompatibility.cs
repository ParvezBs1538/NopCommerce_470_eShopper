using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Data
{
    public partial class NameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(HelpdeskDepartment), "NS_Helpdesk_Department" },
            { typeof(HelpdeskStaff), "NS_Helpdesk_Staff" },
            { typeof(Ticket), "NS_Helpdesk_Ticket" },
            { typeof(TicketResponse), "NS_Helpdesk_TicketResponse" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}