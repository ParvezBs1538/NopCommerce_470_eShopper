using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.CrispChat
{
    public class CrispChatSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string WebsiteId { get; set; }

        public string Script { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}
