using Nop.Core;

namespace NopStation.Plugin.Widgets.Helpdesk.Domains
{
    public class HelpdeskDepartment : BaseEntity
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public int DisplayOrder { get; set; }
    }
}