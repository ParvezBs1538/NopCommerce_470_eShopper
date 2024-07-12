using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.PaldeskChat
{
    public class PaldeskChatSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string Key { get; set; }

        public string Script { get; set; }

        public bool ConfigureWithCustomerDataIfLoggedIn { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}
