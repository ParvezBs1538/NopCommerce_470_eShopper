using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.FacebookMessenger.Models
{
    public class ConfigurationModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookMessenger.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookMessenger.Configuration.Fields.PageId")]
        public string PageId { get; set; }
        public bool PageId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookMessenger.Configuration.Fields.ThemeColor")]
        public string ThemeColor { get; set; }
        public bool ThemeColor_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookMessenger.Configuration.Fields.Script")]
        public string Script { set; get; }
        public bool Script_OverrideForStore { set; get; }

        [NopResourceDisplayName("Admin.NopStation.FacebookMessenger.Configuration.Fields.EnableScript")]
        public bool EnableScript { set; get; }
        public bool EnableScript_OverrideForStore { set; get; }
    }
}
