using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.NopChat
{
    public class NopChatSettings : ISettings
    {
        public int? Logo { get; set; }
        public bool OpenOnMonday { get; set; }
        public bool OpenOnTuesday { get; set; }
        public bool OpenOnWednesday { get; set; }
        public bool OpenOnThursday { get; set; }
        public bool OpenOnFriday { get; set; }
        public bool OpenOnSaturday { get; set; }
        public bool OpenOnSunday { get; set; }
    }
}
