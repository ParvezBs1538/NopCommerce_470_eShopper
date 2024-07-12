using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.LuckyOrange.Models
{
    public class ConfigurationModel
    {
        public ConfigurationModel()
        {
            AvailableSettingModes = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.LuckyOrange.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.LuckyOrange.Configuration.Fields.SiteId")]
        public string SiteId { get; set; }
        public bool SiteId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.LuckyOrange.Configuration.Fields.TrackingCode")]
        public string TrackingCode { set; get; }
        public bool TrackingCode_OverrideForStore { set; get; }

        [NopResourceDisplayName("Admin.NopStation.LuckyOrange.Configuration.Fields.SettingMode")]
        public int SettingModeId { set; get; }
        public bool SettingModeId_OverrideForStore { set; get; }

        public IList<SelectListItem> AvailableSettingModes { set; get; }
    }
}
