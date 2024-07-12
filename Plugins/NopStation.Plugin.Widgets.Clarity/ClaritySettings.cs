using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Clarity
{
    public class ClaritySettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string ProjectId { get; set; }

        public string TrackingCode { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}