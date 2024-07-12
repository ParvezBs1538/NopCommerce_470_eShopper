using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Helpdesk.Models
{
    public record TicketResponseModel : BaseNopEntityModel
    {
        public int TicketId { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.TicketResponses.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.TicketResponses.Download")]
        public int DownloadId { get; set; }

        public Guid DownloadGuid { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.TicketResponses.CreatedByCustomer")]
        public string CreatedByCustomer { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.TicketResponses.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}
