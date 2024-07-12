using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models
{
    public record StaffModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Staffs.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Staffs.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Staffs.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}
