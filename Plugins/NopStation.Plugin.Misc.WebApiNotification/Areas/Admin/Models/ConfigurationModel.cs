using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableApplicationTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.Configuration.Fields.GoogleConsoleApiAccessKey")]
        public string GoogleConsoleApiAccessKey { get; set; }
        public bool GoogleConsoleApiAccessKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.Configuration.Fields.ApplicationTypeId")]
        public int ApplicationTypeId { get; set; }
        public bool ApplicationTypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.Configuration.Fields.PushKitClientSecret")]
        public string PushKitClientSecret { get; set; }
        public bool PushKitClientSecret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.Configuration.Fields.PushKitClientId")]
        public string PushKitClientId { get; set; }
        public bool PushKitClientId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApiNotification.Configuration.Fields.PushKitAppId")]
        public string PushKitAppId { get; set; }
        public bool PushKitAppId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableApplicationTypes { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
