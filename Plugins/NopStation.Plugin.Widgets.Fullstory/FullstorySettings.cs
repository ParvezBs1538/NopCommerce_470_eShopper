using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Fullstory
{
    public class FullstorySettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string OrganizationId { get; set; }

        public string Script { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}
