using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.GoogleTagManager
{
    public class GoogleTagManagerSettings : ISettings
    {
        public bool IsEnable { get; set; }
        public string GTMContainerId { get; set; }
    }
}
