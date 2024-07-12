using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Hotjar
{
    public class HotjarSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string SiteId { get; set; }

        public string Script { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}
