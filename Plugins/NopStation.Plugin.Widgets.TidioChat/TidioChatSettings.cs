using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.TidioChat
{
    public class TidioChatSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string Script { get; set; }
    }
}
