using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.UrlShortener
{
    public class UrlShortenerSettings : ISettings
    {
        public string AccessToken { get; set; }

        public bool EnableLog { get; set; }
    }
}
