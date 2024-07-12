using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.PinterestAnalytics
{
    public class PinterestAnalyticsSettings : ISettings
    {
        public string PinterestId { get; set; }
        public string TrackingScript { get; set; }
        public bool EnableEcommerce { get; set; }
        public string WidgetZone { get; set; }
        public string AdAccountId { get; set; }
        public string AccessToken { get; set; }
        public string ApiUrl { get; set; }
        public bool SaveLog { get; set; }
        public string CustomEvents { get; set; }
    }
}