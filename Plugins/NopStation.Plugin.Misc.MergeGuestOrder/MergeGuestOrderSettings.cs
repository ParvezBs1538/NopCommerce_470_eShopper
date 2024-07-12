using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.MergeGuestOrder
{
    public class MergeGuestOrderSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public bool AddNoteToOrderOnMerge { get; set; }

        public int CheckEmailInAddressId { get; set; }
    }
}