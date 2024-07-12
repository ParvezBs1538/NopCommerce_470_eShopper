using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models
{
    public record GroupNotificationSearchModel : BaseSearchModel
    {
        public GroupNotificationSearchModel()
        {
            AvailableNotificationTemplates = new List<SelectListItem>();
            AvailableGroups = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableNotificationTemplates { get; set; }

        public IList<SelectListItem> AvailableGroups { get; set; }
    }
}
