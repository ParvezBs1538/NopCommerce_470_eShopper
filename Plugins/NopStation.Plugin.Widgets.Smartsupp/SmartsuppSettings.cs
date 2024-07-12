using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Smartsupp
{
    public class SmartsuppSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string Key { get; set; }

        public string Script { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}
