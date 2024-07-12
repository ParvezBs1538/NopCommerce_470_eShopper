using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Models
{
    public record TicketOverviewModel : BaseNopEntityModel
    {
        public Guid TicketGuid { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string Department { get; set; }

        public string Category { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public int DownloadId { get; set; }
        public Guid DownloadGuid { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
