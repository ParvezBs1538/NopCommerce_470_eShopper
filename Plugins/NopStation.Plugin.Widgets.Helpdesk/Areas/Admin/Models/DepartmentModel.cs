using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models
{
    public record DepartmentModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Departments.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Departments.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Helpdesk.Departments.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}
