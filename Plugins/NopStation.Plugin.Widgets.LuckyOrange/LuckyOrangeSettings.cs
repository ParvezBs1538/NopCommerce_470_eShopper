using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.LuckyOrange
{
    public class LuckyOrangeSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string SiteId { get; set; }

        public string TrackingCode { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}
