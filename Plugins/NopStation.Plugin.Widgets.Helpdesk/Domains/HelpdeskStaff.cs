using Nop.Core;

namespace NopStation.Plugin.Widgets.Helpdesk.Domains
{
    public class HelpdeskStaff : BaseEntity
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public int DisplayOrder { get; set; }
    }
}