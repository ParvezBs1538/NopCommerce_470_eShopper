using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models
{
    public record TicketSearchModel : BaseSearchModel
    {
        public TicketSearchModel()
        {
            AvailableCategory = new List<SelectListItem>();
            AvailablePriority = new List<SelectListItem>();
            AvailableStatus = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.List.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.List.Category")]
        public IList<int> CategoryId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.List.Priority")]
        public IList<int> PriorityId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.List.Status")]
        public IList<int> StatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.List.CreatedFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedFrom { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Tickets.List.CreatedTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedTo { get; set; }

        public IList<SelectListItem> AvailableCategory { get; set; }
        public IList<SelectListItem> AvailablePriority { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }
    }
}
