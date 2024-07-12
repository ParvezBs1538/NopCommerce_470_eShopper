using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Mouseflow
{
    public class MouseflowSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string WebsiteId { get; set; }

        public string Script { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}
