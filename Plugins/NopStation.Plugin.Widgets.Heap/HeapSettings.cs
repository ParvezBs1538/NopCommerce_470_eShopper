using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Heap
{
    public class HeapSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string AppId { get; set; }

        public string Script { get; set; }

        public SettingMode SettingMode { get; set; }
    }
}