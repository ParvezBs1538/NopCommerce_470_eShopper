using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Howuku
{
    public class HowukuSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string ProjectKey { get; set; }

        public string Script { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}