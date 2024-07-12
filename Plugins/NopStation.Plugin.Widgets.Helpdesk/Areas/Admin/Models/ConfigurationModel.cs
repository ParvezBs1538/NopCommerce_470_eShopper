using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailablePriorities = new List<SelectListItem>();
            AvailableNavigationWidgetZones = new List<SelectListItem>();
            AvailableOrderPageWidgetZones = new List<SelectListItem>();
            AvailableDepartments = new List<SelectListItem>();
            AvailableEmailAccounts = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.SendEmailOnNewTicket")]
        public bool SendEmailOnNewTicket { get; set; }
        public bool SendEmailOnNewTicket_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.SendEmailOnNewResponse")]
        public bool SendEmailOnNewResponse { get; set; }
        public bool SendEmailOnNewResponse_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.SendEmailsTo")]
        public string SendEmailsTo { get; set; }
        public bool SendEmailsTo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.EmailAccountId")]
        public int EmailAccountId { get; set; }
        public bool EmailAccountId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToSetPriority")]
        public bool AllowCustomerToSetPriority { get; set; }
        public bool AllowCustomerToSetPriority_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.DefaultTicketPriorityId")]
        public int DefaultTicketPriorityId { get; set; }
        public bool DefaultTicketPriorityId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToUploadAttachmentInTicket")]
        public bool AllowCustomerToUploadAttachmentInTicket { get; set; }
        public bool AllowCustomerToUploadAttachmentInTicket_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToUploadAttachmentInResponse")]
        public bool AllowCustomerToUploadAttachmentInResponse { get; set; }
        public bool AllowCustomerToUploadAttachmentInResponse_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.EnableTicketDepartment")]
        public bool EnableTicketDepartment { get; set; }
        public bool EnableTicketDepartment_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.TicketDepartmentRequired")]
        public bool TicketDepartmentRequired { get; set; }
        public bool TicketDepartmentRequired_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.DefaultTicketDepartmentId")]
        public int DefaultTicketDepartmentId { get; set; }
        public bool DefaultTicketDepartmentId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.EnableTicketCategory")]
        public bool EnableTicketCategory { get; set; }
        public bool EnableTicketCategory_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.TicketCategoryRequired")]
        public bool TicketCategoryRequired { get; set; }
        public bool TicketCategoryRequired_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.DefaultTicketCategoryId")]
        public int DefaultTicketCategoryId { get; set; }
        public bool DefaultTicketCategoryId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.ShowMenuInCustomerNavigation")]
        public bool ShowMenuInCustomerNavigation { get; set; }
        public bool ShowMenuInCustomerNavigation_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.NavigationWidgetZone")]
        public string NavigationWidgetZone { get; set; }
        public bool NavigationWidgetZone_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToCreateTicketFromOrderPage")]
        public bool AllowCustomerToCreateTicketFromOrderPage { get; set; }
        public bool AllowCustomerToCreateTicketFromOrderPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.OrderPageWidgetZone")]
        public string OrderPageWidgetZone { get; set; }
        public bool OrderPageWidgetZone_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.MinimumTicketCreateInterval")]
        public int MinimumTicketCreateInterval { get; set; }
        public bool MinimumTicketCreateInterval_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Configuration.Fields.MinimumResponseCreateInterval")]
        public int MinimumResponseCreateInterval { get; set; }
        public bool MinimumResponseCreateInterval_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailablePriorities { get; set; }
        public IList<SelectListItem> AvailableDepartments { get; set; }
        public IList<SelectListItem> AvailableNavigationWidgetZones { get; set; }
        public IList<SelectListItem> AvailableOrderPageWidgetZones { get; set; }
        public IList<SelectListItem> AvailableEmailAccounts { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
