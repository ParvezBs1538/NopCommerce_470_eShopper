using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models
{
    public record WebApiNotificationTemplateSearchModel : BaseSearchModel
    {
        public WebApiNotificationTemplateSearchModel()
        {
            AvailableActiveTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchKeyword")]
        public string SearchKeyword { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.PushNotificationTemplates.List.SearchActiveId")]
        public int SearchActiveId { get; set; }

        public IList<SelectListItem> AvailableActiveTypes { get; set; }
    }
}
