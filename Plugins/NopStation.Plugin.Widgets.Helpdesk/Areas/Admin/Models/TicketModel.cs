using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models
{
    public record TicketModel : BaseNopEntityModel
    {
        public TicketModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableDepartments = new List<SelectListItem>();
            AvailableStaffs = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailablePriorities = new List<SelectListItem>();
            AvailableStatuses = new List<SelectListItem>();
            ResponseSearchModel = new ResponseSearchModel();
            ResponseAddModel = new ResponseModel();
        }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.TicketGuid")]
        public Guid TicketGuid { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.Subject")]
        public string Subject { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.DepartmentId")]
        public int DepartmentId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.StaffId")]
        public int StaffId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.StoreId")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.CustomerId")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.CategoryId")]
        public int CategoryId { get; set; }
        public string Category { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.StatusId")]
        public int StatusId { get; set; }
        public string Status { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.PriorityId")]
        public int PriorityId { get; set; }
        public string Priority { get; set; }

        [UIHint("Download")]
        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.DownloadId")]
        public int DownloadId { get; set; }
        public Guid DownloadGuid { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.CreatedBy")]
        public int CreatedByCustomerId { get; set; }
        public string CreatedByCustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableDepartments { get; set; }
        public IList<SelectListItem> AvailableStaffs { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailablePriorities { get; set; }
        public IList<SelectListItem> AvailableStatuses { get; set; }

        public ResponseSearchModel ResponseSearchModel { get; set; }

        public ResponseModel ResponseAddModel { get; set; }
    }
}
