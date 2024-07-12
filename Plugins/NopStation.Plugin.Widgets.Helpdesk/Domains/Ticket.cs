using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.Helpdesk.Domains
{
    public class Ticket : BaseEntity
    {
        public Guid TicketGuid { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public int DepartmentId { get; set; }

        public int StoreId { get; set; }

        public int DownloadId { get; set; }

        public int CustomerId { get; set; }

        public int StaffId { get; set; }

        public int CategoryId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int StatusId { get; set; }

        public int PriorityId { get; set; }

        public int CreatedByCustomerId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }


        public TicketStatus Status
        {
            get => (TicketStatus)StatusId;
            set => StatusId = (int)value;
        }

        public TicketPriority Priority
        {
            get => (TicketPriority)PriorityId;
            set => PriorityId = (int)value;
        }

        public TicketCategory Category
        {
            get => (TicketCategory)CategoryId;
            set => CategoryId = (int)value;
        }
    }
}