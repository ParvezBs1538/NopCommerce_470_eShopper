using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.Helpdesk.Domains
{
    public class TicketResponse : BaseEntity
    {
        public int TicketId { get; set; }

        public string Body { get; set; }

        public int DownloadId { get; set; }

        public bool DisplayToCustomer { get; set; }

        public int CreatedByCustomerId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}