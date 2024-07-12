using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PaldeskChat.Models
{
    public class ConfigurationModel
    {
        public ConfigurationModel()
        {
            AvailableSettingModes = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PaldeskChat.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PaldeskChat.Configuration.Fields.Key")]
        public string Key { get; set; }
        public bool Key_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PaldeskChat.Configuration.Fields.Script")]
        public string Script { set; get; }
        public bool Script_OverrideForStore { set; get; }

        [NopResourceDisplayName("Admin.NopStation.PaldeskChat.Configuration.Fields.SettingMode")]
        public int SettingModeId { set; get; }
        public bool SettingModeId_OverrideForStore { set; get; }

        [NopResourceDisplayName("Admin.NopStation.PaldeskChat.Configuration.Fields.ConfigureWithCustomerDataIfLoggedIn")]
        public bool ConfigureWithCustomerDataIfLoggedIn { get; set; }
        public bool ConfigureWithCustomerDataIfLoggedIn_OverrideForStore { set; get; }

        public IList<SelectListItem> AvailableSettingModes { set; get; }
    }
}
