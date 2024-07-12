using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Smartlook
{
    public class SmartlookSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string ProjectKey { get; set; }

        public string Script { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}
