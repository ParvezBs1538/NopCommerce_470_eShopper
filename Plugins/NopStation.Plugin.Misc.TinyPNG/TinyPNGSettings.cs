using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.TinyPNG
{
    public  class TinyPNGSettings : ISettings 
    {
        public bool TinyPNGEnable { get; set; }

        public string ApiUrl { get; set; }

        public string Keys { get; set; }
    }
}
