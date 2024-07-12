using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Helpdesk.Models
{
    public record TicketModel : BaseNopEntityModel
    {
        public TicketModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableDepartments = new List<SelectListItem>();
            AvailablePriorities = new List<SelectListItem>();
            AvailableOrders = new List<SelectListItem>();
        }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.TicketGuid")]
        public Guid TicketGuid { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Subject")]
        public string Subject { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Department")]
        public int DepartmentId { get; set; }
        public string Department { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Category")]
        public int CategoryId { get; set; }
        public string Category { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Order")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Status")]
        public string Status { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Priority")]
        public int PriorityId { get; set; }
        public string Priority { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Download")]
        public int DownloadId { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.Download")]
        public Guid DownloadGuid { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("NopStation.Helpdesk.Tickets.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        public bool EnableTicketDepartment { get; set; }
        public bool TicketDepartmentRequired { get; set; }
        public bool EnableTicketCategory { get; set; }
        public bool TicketCategoryRequired { get; set; }
        public bool AllowCustomerToSetPriority { get; set; }
        public bool AllowCustomerToUploadAttachmentInTicket { get; set; }
        public bool AllowCustomerToUploadAttachmentInResponse { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableDepartments { get; set; }
        public IList<SelectListItem> AvailablePriorities { get; set; }
        public IList<SelectListItem> AvailableOrders { get; set; }
    }
}
