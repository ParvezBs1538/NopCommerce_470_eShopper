using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Smartlook.Models
{
    public class ConfigurationModel
    {
        public ConfigurationModel()
        {
            AvailableSettingModes = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Smartlook.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Smartlook.Configuration.Fields.ProjectKey")]
        public string ProjectKey { get; set; }
        public bool ProjectKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Smartlook.Configuration.Fields.Script")]
        public string Script { set; get; }
        public bool Script_OverrideForStore { set; get; }

        [NopResourceDisplayName("Admin.NopStation.Smartlook.Configuration.Fields.SettingMode")]
        public int SettingModeId { set; get; }
        public bool SettingModeId_OverrideForStore { set; get; }

        public IList<SelectListItem> AvailableSettingModes { set; get; }
    }
}
