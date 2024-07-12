using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models
{
    public record ResponseModel : BaseNopEntityModel
    {
        public int TicketId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Responses.Fields.Body")]
        public string Body { get; set; }

        [UIHint("Download")]
        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Responses.Fields.DownloadId")]
        public int DownloadId { get; set; }
        public Guid DownloadGuid { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Responses.Fields.DisplayToCustomer")]
        public bool DisplayToCustomer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Responses.Fields.CreatedBy")]
        public int CreatedByCustomerId { get; set; }
        public string CreatedByCustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Responses.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}
