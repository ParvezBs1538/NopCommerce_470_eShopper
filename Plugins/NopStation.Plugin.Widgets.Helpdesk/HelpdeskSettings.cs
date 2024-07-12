using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Helpdesk
{
    public class HelpdeskSettings : ISettings
    {
        public bool SendEmailOnNewTicket { get; set; }

        public bool SendEmailOnNewResponse { get; set; }

        public string SendEmailsTo { get; set; }

        public int EmailAccountId { get; set; }

        public bool AllowCustomerToSetPriority { get; set; }

        public int DefaultTicketPriorityId { get; set; }

        public bool AllowCustomerToUploadAttachmentInTicket { get; set; }

        public bool AllowCustomerToUploadAttachmentInResponse { get; set; }

        public bool EnableTicketDepartment { get; set; }

        public bool TicketDepartmentRequired { get; set; }

        public int DefaultTicketDepartmentId { get; set; }

        public bool EnableTicketCategory { get; set; }

        public bool TicketCategoryRequired { get; set; }

        public int DefaultTicketCategoryId { get; set; }

        public bool ShowMenuInCustomerNavigation { get; set; }

        public string NavigationWidgetZone { get; set; }

        public bool AllowCustomerToCreateTicketFromOrderPage { get; set; }

        public string OrderPageWidgetZone { get; set; }

        public int MinimumTicketCreateInterval { get; set; }

        public int MinimumResponseCreateInterval { get; set; }
    }
}